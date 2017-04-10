using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteFile
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
        
        public static void Main()
        {
            Console.WriteLine("Starting a Client");

            SocketAdapter socketAdapter = new SocketAdapter();
            FileManager fileManager = new FileManager();

            socketAdapter.setRecieveHandler(fileManager);
            Thread socketAdapterThread = new Thread(new ThreadStart(socketAdapter.work));
            //socketAdapterThread.IsBackground = true;
            socketAdapterThread.Name = "Clients socketAdapterThread";
            socketAdapterThread.Start();

            Thread fileManagerThread = new Thread(new ThreadStart(fileManager.work));
            fileManagerThread.IsBackground = true;
            fileManagerThread.Name = "Clients fileManagerThread";
            fileManagerThread.Start();

            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("tick from Client: " + Thread.CurrentThread.Name);
            }
        }

        public void test()
        {

        }
    }
}
