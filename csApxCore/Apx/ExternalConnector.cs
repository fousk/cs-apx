using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Apx
{
    
    public class ExternalConnector
    {
        public static ConcurrentQueue<ExternalMsg> msgs = new ConcurrentQueue<ExternalMsg>();
        static bool isHostConnected = false;
        
        public static void enqueue(ExternalMsg msg)
        {
            msgs.Enqueue(msg);
        }

        public static ExternalMsg dequeue()
        {
            ExternalMsg msg;
            msgs.TryDequeue(out msg);
            return msg;
        }
    }

    public class ExternalMsg
    {
        public string name;
        public string value;

        public ExternalMsg(string setName, string setValue)
        {
            name = setName;
            value = setValue;
        }
    }
}
