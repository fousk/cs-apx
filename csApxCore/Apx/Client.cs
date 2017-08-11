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
        public static ConcurrentQueue<ExternalMsg> externalMsgs; // = new ConcurrentQueue<ExternalMsg>();

        public void Main(string ipAddress = "127.0.0.1", int port = 5000)
        { 
            Thread.CurrentThread.Name = "MainThread";

            nodeData = new NodeData("startupPath");
            if (externalMsgs != null)
                nodeData.setExternalQueue(externalMsgs);
            fileManager = new Apx.FileManager();
            fileManager.attachNodeData(nodeData);
            fileManager.start();

            try
            {
                bool connectRes = connectTcp(ipAddress, port);
                if (connectRes)
                    tryEnqueue(new ExternalMsg("status", "connected"));
                while (connectRes)
                {
                    Thread.Sleep(1000);
                }
                tryEnqueue(new ExternalMsg("status", "notConnected"));
            }
            catch (Exception e)
            {
                tryEnqueue(new ExternalMsg("status", "notConnected"));
                Console.WriteLine(e.ToString());
            }
            
        }

        public void setQueue(ConcurrentQueue<ExternalMsg> setMsgs)
        {
            externalMsgs = setMsgs;
        }

        static void tryEnqueue(ExternalMsg msg)
        {
            if (externalMsgs != null)
                externalMsgs.Enqueue(msg);
        }

        static public bool connectTcp(string address, int port)
        {
            socketAdapter.setRecieveHandler(fileManager);
            if (socketAdapter.connect(address, port, 0))
            {
                socketAdapterThread = new Thread(new ThreadStart(socketAdapter.worker));
                socketAdapterThread.Name = "Clients socketAdapterThread";
                socketAdapterThread.IsBackground = true;
                socketAdapterThread.Start();
                return true;
            }
            else
            { return false; }
        }

        public void close()
        {
            nodeData = null;
            fileManager.stop();
            fileManager = null;
        }
    }
}
