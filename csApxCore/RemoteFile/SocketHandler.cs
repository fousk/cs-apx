﻿using System;
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
        /*
        public virtual void send(byte[] header, byte[] data)
        { }
        public virtual void send(byte[] header, List<byte> data)
        { }
        public virtual void send(List<byte> header, List<byte> data)
        { }
         */
        public virtual void send(List<byte> data)
        { }
    }


    public class SocketAdapter : TransmitHandler
    {
        static ReceiveHandler receiveHandler;
        static bool isAcknowledgeSeen = false;
        static bool isConnected = false;

        static TcpClient client;
        static NetworkStream tcpStream; 


        public SocketAdapter()
        { }


        public void worker()
        {
            List<byte> unprocessed = new List<byte>();
            byte[] buffer = new byte[2048]; // read in chunks of 2KB
            int bytesRead = 0;
            int bytesParsed;

            while (true)
            {
                if (isConnected)
                {
                    try
                    {
                        if (tcpStream.CanRead)
                        {
                            byte[] readBuffer = new byte[2048];
                            bytesRead = tcpStream.Read(readBuffer, 0, readBuffer.Length);
                            if (bytesRead <= 0)
                            { break; }
                            unprocessed.AddRange(readBuffer.Take(bytesRead));
                            bytesParsed = _parseData(unprocessed);
                            //Console.WriteLine("- bytesParsed" + bytesParsed);
                            if (bytesParsed > 0)
                            {
                                unprocessed.RemoveRange(0, bytesParsed);
                            }
                            else if (bytesParsed < 0)
                            { throw new ArgumentException("TcpSocketAdapter._parseData error "  + bytesParsed); }
                        }
                    }
                    catch (Exception e)
                    { Console.WriteLine(e.ToString()); }
                }
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
                if (iNext < iBegin)
                {
                    throw new ArgumentException("remotefile.socket_adapter._parseData failure\n");
                }
                else if (iNext == iBegin)
                {
                    break;  // wait for more data to arrive before parsing again                
                }
                else
                {
                    iBegin = iNext;
                }
            }
            return iBegin;
        }


        public int _parseMessage(List<byte> data, int iBegin)
        {
            NumHeader.decodeReturn ret = NumHeader.decode(data, iBegin, 32);
            int iNext;
            if (ret.bytesParsed < 0)
            { return ret.bytesParsed;  }
            else if (ret.bytesParsed == 0)
            { return iBegin; }
            else
            {
                iNext = iBegin + (int)ret.bytesParsed;
                if (iNext + ret.value <= data.Count)
                {
                    List<byte> msg = data.GetRange(iNext, (int)ret.value);
                    if (isAcknowledgeSeen == false)
                    {
                        if (msg.Count == 8 && (msg.SequenceEqual(new byte[] { 0xbf, 0xff, 0xfc, 0x00, 0x00, 0x00, 0x00, 0x00 })))
                        {
                            // We are connected
                            isAcknowledgeSeen = true;
                            receiveHandler.onConnected(this);
                            return iNext + (int)ret.value;
                        }
                        else
                        { throw new ArgumentException("expected acknowledge from apx_server but something else"); }
                    }
                    else if (receiveHandler != null)
                    {
                        receiveHandler.onMsgReceived(msg);
                        return iNext + (int)ret.value;
                    }
                    else
                    { throw new ArgumentException("receiveHandler is null"); }
                }
                else
                { return iBegin; }
            }
        }


        public bool connect(string address, int port, int retries = 1)
        {
            client = new TcpClient();
            if (address == "localhost")
            { address = "127.0.0.1"; }
            System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(address);  //127.0.0.1 as an example
            //bool connected = false;
            //int numberOfRetries = 0;
            int maxNumberOfRetries = retries;     // Set 0 for infinite retries
            try
            {
                Console.WriteLine("Connecting to target");
                /*while (!connected)
                {
                    try
                    { 
                        client.Connect(ipaddress, port);
                        connected = true;
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Connection failed, retrying...");
                        numberOfRetries++;
                        if ((numberOfRetries >= maxNumberOfRetries) && (maxNumberOfRetries != 0))
                            break;
                    }
                }*/
                client.Connect(ipaddress, port);
                Console.WriteLine("Connected to target");
                tcpStream = client.GetStream();
                isConnected = true;
                Console.WriteLine("Connected to: " + address + ", port: " + port.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                tcpStream.Close();
                client.Close();
                return false;
            }

            List<byte> greeting = ASCIIEncoding.ASCII.GetBytes(Apx.Constants.defaultGreeting).ToList();
            send(greeting);
            return true;
        }


        public void setRecieveHandler(ReceiveHandler handler)
        {
            receiveHandler = handler;
        }


        public void sendTransmitHandler()
        {
            receiveHandler.onConnected(this);
        }


        public override void send(List<byte> msg)
        {
            List<byte> data = NumHeader.encode((uint)msg.Count, 32);
            data.AddRange(msg);
            Console.WriteLine("sending: " + data.Count + " bytes");
            tcpStream.Write(data.ToArray(), 0, data.Count);
            //Console.WriteLine("_Data_" + Encoding.Default.GetString(data.ToArray()) + "_End_");
        }
    }
}
