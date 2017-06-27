using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteFile;

namespace Apx
{
    public class Client
    {
        /*SocketAdapter socketAdapter = new SocketAdapter();
        FileManager fileManager = new FileManager();
        //socketAdapter.setRecieveHandler(fileManager);
        Client()
        {
            Thread sock = new Thread(new ThreadStart(socketAdapter.work()));

            socketAdapter.setRecieveHandler(fileManager);
        }
        */
        static SocketAdapter socketAdapter = new SocketAdapter();
        static Apx.FileManager fileManager; // = new Apx.FileManager(); // [ToDo] create real fileMaps
        static Thread socketAdapterThread;
        static NodeData nodeData;


        public static void Main()
        {
            Thread.CurrentThread.Name = "MainThread";

            nodeData = new NodeData("startupPath");

            fileManager = new FileManager();
            fileManager.attachNodeData(nodeData);
            fileManager.start();

            try
            {
                //bool connectRes = connectTcp("127.0.0.1", 5000);
                bool connectRes = connectTcp("192.168.137.123", 5000);
                while (true)
                {
                    Thread.Sleep(1000);
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
            if (socketAdapter.connect(address, port))
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
