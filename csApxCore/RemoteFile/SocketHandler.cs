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
        
        static TcpListener server;
        static Socket socket = server.AcceptSocket();
        static Stream stream = new NetworkStream(socket);
        static StreamWriter writer = new StreamWriter(stream);
        static StreamReader reader = new StreamReader(stream);

        public SocketAdapter()
        {
            this.InstanceID = Guid.NewGuid();
        }

        public void worker()
        {
            Console.WriteLine("Connected " + socket.RemoteEndPoint);
            writer.AutoFlush = true;
            List<byte> unprocessed = new List<byte>();

            while(true)
            {
                try
                {
                    //read = reader.ReadToEnd();
                    //unprocessed.AddRange(read.)
                }
                catch (Exception e)
                { Console.WriteLine(e.ToString()); }

                throw new NotImplementedException();
            }
        }

        public uint _parseData(List<byte> data)
        {
            uint pos = 0;
            uint next;
            uint end = (uint)data.Count;
            while (pos < end)
            {
                next = _parseMessage(data, pos);
            }

            throw new NotImplementedException();
        }

        public uint _parseMessage(List<byte> data, uint pos)
        {
            throw new NotImplementedException();
            
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
            socket.Send(package);
        }
    }
}
