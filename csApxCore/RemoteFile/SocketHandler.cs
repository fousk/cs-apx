using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;



namespace RemoteFile
{
    public class TransmitHandler
    {
        public virtual void send(byte[] header, byte[] data)
        { }
        public virtual void send(byte[] header, List<byte> data)
        { }
        public virtual void send(List<byte> header, List<byte> data)
        { }
        public virtual void send(List<byte> data)
        { }
    }

    public class SocketAdapter : TransmitHandler
    {
        public Guid InstanceID { get; private set; }    // Check that we use the right instance
        ReceiveHandler receiveHandler;
        bool isAcknowledgeSeen = false;
        bool isConnected = false;

        static TcpClient client = new TcpClient();
        static Stream tcpStream = client.GetStream();

        public SocketAdapter()
        {
            this.InstanceID = Guid.NewGuid();
        }

        public void worker()
        {
            List<byte> unprocessed = new List<byte>();
            byte[] buffer = new byte[2048]; // read in chunks of 2KB
            int bytesRead;
            int result;

            using (var memStream = new MemoryStream())
            {
                while (true)
                {
                    try
                    {
                        while ((bytesRead = tcpStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memStream.Write(buffer, 0, bytesRead);
                        }
                        unprocessed.AddRange(memStream.ToArray());
                        result = _parseData(unprocessed);
                        if (result < 0)
                        {
                            Console.WriteLine("TcpSocketAdapter._parseData error: " + result.ToString());
                            break;
                        }
                        else if (result > 0)
                        {
                            unprocessed.RemoveRange(0, result);
                        }
                        else
                        { 
                            // Loop
                        }
                    }
                    catch (Exception e)
                    { Console.WriteLine(e.ToString()); }
                }
            }
        }

        public bool connect(string address, int port)
        {
            if (address == "localhost")
            { address = "127.0.0.1"; }
            System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(address);  //127.0.0.1 as an example
            try
            {
                client.Connect(ipaddress, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public int _parseData(List<byte> data)
        {
            int iBegin = 0;
            int iNext;
            int iEnd = data.Count;
            while (iBegin < iEnd)
            {
                iNext = _parseMessage(data, iBegin);
                if (iNext == iBegin)
                {
                    // wait for more data to arrive before parsing again
                    break;
                }
                else if (iNext >= iBegin)
                {
                    iBegin = iNext;
                }
                else
                {
                    Console.WriteLine("remotefile.socket_adapter._parseData failure\n");
                    return -1;
                }
            }
            return iBegin;
        }

        public int _parseMessage(List<byte> data, int iBegin)
        {
            NumHeader.decodeReturn ret = NumHeader._decode(data, iBegin, 32);
            int iNext;
            if (ret.bytesParsed == 0)
            {
                return iBegin;
            }
            else
            {
                iNext = iBegin + (int)ret.bytesParsed;
                if (iNext + ret.value <= data.Count)
                {
                    List<byte> msg = data.GetRange(iNext, iNext + (int)ret.value);
                    if (isAcknowledgeSeen == false)
                    {
                        if (msg.Count == 8)
                        {
                            if (msg.SequenceEqual(new byte[] { 0xbf, 0xff, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00 }))
                            {
                                // We are connected
                                isAcknowledgeSeen = true;
                                receiveHandler.onConnected(this);
                            }
                            else
                            { throw new ArgumentException("expected acknowledge from apx_server but something else"); }
                        }
                        throw new ArgumentException("expected acknowledge from apx_server but something else");
                    }
                    else
                    {
                        receiveHandler.onMsgReceived(msg);
                    }
                }
                return iBegin;
            }
        }

        public void setRecieveHandler(ReceiveHandler handler)
        {
            receiveHandler = handler;
        }

        public void sendTransmitHandler()
        {
            receiveHandler.onConnected(this);
        }
        // after connect is OK
        // handler.onConnected(this) // skickar TransmitHandler delen

        public override void send(byte[] header, byte[] data)
        {
            // Some functions return byteArrays as headers
            List<byte> newHeader = header.Cast<byte>().ToList();
            List<byte> newData = data.Cast<byte>().ToList();
            send(newHeader, newData);
        }
        public override void send(byte[] header, List<byte> data)
        {
            // Some functions return byteArrays as headers
            List<byte> newHeader = header.Cast<byte>().ToList();
            send(newHeader, data);
        }
        public override void send(List<byte> header, List<byte> data)
        {
            // to avoid concatenating arrays in the main code.
            List<byte> tempData = new List<byte>();
            tempData.AddRange(header);
            tempData.AddRange(data);
            send(tempData);
        }
        public override void send(List<byte> data)
        {
            // Add length header
            byte[] lenHeader = BitConverter.GetBytes((uint)data.Count);
            byte[] package = new byte[4 + data.Count];
            lenHeader.CopyTo(package, 0);
            data.ToArray().CopyTo(package, 4);
            tcpStream.Write(package, 0, package.Length);
        }
    }
}
