using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteFile;
using Apx;

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
                Thread.Sleep(1000);
            }
        }
    }
}
