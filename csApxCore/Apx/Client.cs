using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteFile;
using System.Collections.Concurrent;

namespace Apx
{
    public class Client
    {
        static SocketAdapter socketAdapter = new SocketAdapter();
        static Thread socketAdapterThread;
        static Apx.FileManager fileManager;
        static NodeData nodeData;
        // If cs-apx is created from another instance, Eg. CANoe
        public static ConcurrentQueue<ExternalMsg> externalMsgs = new ConcurrentQueue<ExternalMsg>();

        public void Main(string ipAddress = "127.0.0.1", int port = 5000)
        { 
            Thread.CurrentThread.Name = "MainThread";

            nodeData = new NodeData("startupPath");
            nodeData.setExternalQueue(externalMsgs);
            fileManager = new Apx.FileManager();
            fileManager.attachNodeData(nodeData);
            fileManager.start();

            try
            {
                bool connectRes = connectTcp(ipAddress, port);
                while (true)
                {
                    Thread.Sleep(1000);
                    /*Console.WriteLine("buffer at size: " + externalMsgs.Count.ToString());
                    /ExternalMsg m;
                    if (externalMsgs.Count > 0)
                    {
                        externalMsgs.TryDequeue(out m);
                        if (m != null)
                        {
                            Console.WriteLine(m.name + " " + m.value);
                        }
                    }*/
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }
        
        static public bool connectTcp(string address, int port)
        {
            socketAdapter.setRecieveHandler(fileManager);
            if (socketAdapter.connect(address, port, 0))
            {
                socketAdapterThread = new Thread(new ThreadStart(socketAdapter.worker));
                socketAdapterThread.Name = "Clients socketAdapterThread";
                socketAdapterThread.Start();
                return true;
            }
            else
            { return false; }
        }

    }
}
