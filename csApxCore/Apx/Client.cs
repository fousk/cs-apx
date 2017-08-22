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
        static SocketAdapter socketAdapter;
        static Thread socketAdapterThread;
        static Apx.FileManager fileManager;
        static NodeData nodeData;
        static bool running;
        // If cs-apx is created from another instance, Eg. CANoe
        public static ConcurrentQueue<ExternalMsg> externalMsgs; // = new ConcurrentQueue<ExternalMsg>();

        public void Main(string ipAddress = "127.0.0.1", int port = 5000)
        {
            running = true;
            Thread.CurrentThread.Name = "MainThread";

            startSubClasses();
            try 
            {
                bool connectRes = connectTcp(ipAddress, port);
                if (connectRes )
                    tryEnqueue(new ExternalMsg("status", "connected"));
                while (connectRes && running)
                {
                    Thread.Sleep(1000);
                    socketAdapter.ping();
                    if (socketAdapterThread.ThreadState == ThreadState.Stopped)
                    {
                        running = false;
                        Console.WriteLine("socketAdapterThread stopped, closing Client");
                    }
                        
                }
                tryEnqueue(new ExternalMsg("status", "notConnected"));
            }
            catch (Exception e)
            {
                tryEnqueue(new ExternalMsg("status", "notConnected"));
                Console.WriteLine(e.ToString());
                running = false;
            }
        }

        public void startSubClasses()
        {
            if (socketAdapter == null)
                socketAdapter = new SocketAdapter();
            if (nodeData == null)
                nodeData = new NodeData("startupPath");
            if (fileManager == null)
                fileManager = new Apx.FileManager();
            if (externalMsgs != null)
                nodeData.setExternalQueue(externalMsgs);
            fileManager.attachNodeData(nodeData);
            fileManager.start();
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
            if (socketAdapter == null)
                socketAdapter = new SocketAdapter();
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
            Console.WriteLine("closing APX client");
            if (socketAdapter != null )
                socketAdapter.disconnect();
            nodeData = null;
            if (fileManager != null)
            {
                fileManager.stop();
                fileManager = null;
            }
            running = false;
        }
    }
}
