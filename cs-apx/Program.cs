using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteFile;

namespace cs_apx
{
    class Program
    {
        static void Main(string[] args)
        {
            //Client client = new Client();
            Client.Main();

            while (true)
            {
                Console.WriteLine("tick from Main: " + Thread.CurrentThread.Name);
                Thread.Sleep(1000);
            }
        }
    }
}
