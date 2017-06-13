using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apx;

namespace Apx
{
    class NodeData
    {
        public Apx.File inPortDataFile;
        public Apx.File outPortDataFile;
        public Apx.File definitionFile;

        public NodeData(string nodeName, uint indataLen, uint outdataLen, string definition)
        {
            if (indataLen > 0)
            { createInPortDataFile(nodeName, indataLen); }
            if (outdataLen > 0)
            { createOutPortDataFile(nodeName, outdataLen); }
            if (definition.Length > 0)
            { createDefinitionFile(nodeName, definition); }
            else
            {
                throw new ArgumentException("Definition string must not be empty");
            }
        }
        
        private void createInPortDataFile(string nodeName, uint indataLen)
        {
            inPortDataFile = new Apx.File(nodeName + ".in", indataLen);
        }

        private void createOutPortDataFile(string nodeName, uint outDataLen)
        {
            outPortDataFile = new Apx.File(nodeName + ".out", outDataLen);
        }

        private void createDefinitionFile(string nodeName, string definition)
        {
            List<Byte> definitionBytes = new List<byte>();
            definitionBytes.AddRange(ASCIIEncoding.ASCII.GetBytes(definition));
            definitionFile = new Apx.File(nodeName + ".apx", (uint)definitionBytes.Count);
            definitionFile.write(0, definitionBytes);
            Console.WriteLine("definitionFile length: " + definitionFile.length);
        }

    }
}
