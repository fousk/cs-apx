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
        static FileMap localFileMap = new FileMap();
        static FileMap remoteFileMap = new FileMap();
        static FileManager fileManager = new FileManager(localFileMap, remoteFileMap); // [ToDo] create real fileMaps
        static Thread socketAdapterThread;

        public static void Main()
        {
            Thread.CurrentThread.Name = "MainThread";
            Console.WriteLine("Starting a Client (" + Thread.CurrentThread.Name + ")");

            try
            {
                bool connectRes = connectTcp("127.0.0.1", 5000);
                /* Fix propperly
                Thread fileManagerThread = new Thread(new ThreadStart(fileManager.worker));
                fileManagerThread.IsBackground = true;
                fileManagerThread.Name = "Clients fileManagerThread";
                fileManagerThread.Start();
                */
                while (true)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("tick from Client: " + Thread.CurrentThread.Name);
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
