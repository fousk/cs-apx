using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteFile;
using Apx;

namespace cs_apx
{
    class Program
    {
        static Thread apxCoreThread;
        static Client apxClient;
        static ConcurrentQueue<ExternalMsg> msgs = new ConcurrentQueue<ExternalMsg>();
        static bool isConnected = false;

        static void Main(string[] args)
        {
            int chunk = 0;
            startClient();

            ExternalMsg msg;
            while (true)
            {
                Thread.Sleep(1000);
                chunk = msgs.Count;
                if (chunk >= 10)
                    Console.WriteLine("chunk of " + chunk + " messages");
                while(!msgs.IsEmpty)
                {
                    msgs.TryDequeue(out msg);
                    handleMsg(msg, chunk);
                }
                if (apxCoreThread.ThreadState == ThreadState.Stopped)
                {
                    stopClient();
                    startClient();
                }
                    
                Console.WriteLine(apxCoreThread.ThreadState.ToString());
            }
        }

        static void handleMsg(ExternalMsg msg, int chunksize)
        {
            if (msg.name == "status")
            {
                Console.WriteLine("Dequeued: " + msg.name + " - " + msg.value);
                if (msg.value == "connected")
                {
                    isConnected = true;
                    Console.WriteLine("--- CONNECTED ---");
                }
                else if (msg.value == "connected")
                {
                    isConnected = false;
                    Console.WriteLine("--- NOT CONNECTED ---");
                }
            }
            else
            {
                if (chunksize < 10)
                    Console.WriteLine("Dequeued: " + msg.name + " - " + msg.value);
            }
        }

        static void startClient()
        {
            apxClient = new Client();
            apxClient.setQueue(msgs);
            //apxCoreThread = new Thread(() => apxClient.Main("localhost", 5000));
            apxCoreThread = new Thread(() => apxClient.Main("192.168.137.123", 5000));
            apxCoreThread.IsBackground = true;
            apxCoreThread.Start();
        }

        static void stopClient()
        {
            Console.WriteLine("stopping APX client");
            apxClient.close();
            apxCoreThread.Interrupt();
            apxCoreThread.Join();
        }
    }
}
