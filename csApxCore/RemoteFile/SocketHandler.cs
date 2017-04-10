using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteFile
{
    public class TransmitHandler
    {

    }

    public class SocketAdapter : TransmitHandler
    {
        ReceiveHandler receiveHandler;

        public void work()
        {
            // Do stuff
            while(true)
            {
                Console.WriteLine("tick from SocketAdapter: " + Thread.CurrentThread.Name);
                Thread.Sleep(1000);
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
    }
}
