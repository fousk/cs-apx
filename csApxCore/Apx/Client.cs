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
            Console.WriteLine("Starting a Client (" + Thread.CurrentThread.Name + ")");

            //nodeData = new NodeData("dummyNode", 2, 1, Apx.Constants.definition);
            //nodeData = new NodeData("dummyNode", 3, 1, Apx.Constants.definition);
            nodeData = new NodeData();

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
                    //Console.WriteLine("tick from Client: " + Thread.CurrentThread.Name);
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
