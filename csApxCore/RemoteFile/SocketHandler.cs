using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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

        public SocketAdapter()
        {
            this.InstanceID = Guid.NewGuid();
        }

        public void worker()
        {
            // Do stuff
            while(true)
            {
                //Console.WriteLine("tick from SocketAdapter: " + Thread.CurrentThread.Name);
                Console.WriteLine("tick from SocketAdapter: " + InstanceID);
                Thread.Sleep(1000);
                receiveHandler.onConnected(this);
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
            throw new System.NotImplementedException();
        }
    }
}
