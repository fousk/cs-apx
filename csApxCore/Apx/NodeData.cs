using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Apx;

namespace Apx
{
    public class NodeData : Apx.FileEventHandler
    {
        public Apx.File inPortDataFile;
        public Apx.File outPortDataFile;
        public Apx.File definitionFile;
        string nodeName;

        public void onFileWrite(File file, uint offset, List<byte> data)
        {
            Console.WriteLine("File name: " + file.name);
            Console.WriteLine("NodeData: " + nodeName);
            Console.WriteLine("offset: " + offset);
            Console.WriteLine("data:" + BitConverter.ToString(data.ToArray()));

            //interprate(
        }

        public NodeData(string path = "default")
        {
            nodeName = "csApxClient";
            uint indataLen = 0;
            uint outdataLen = 0;
            string definition = "";

            if (path == "default")
            { path = Apx.Constants.defaultDefinitionPath; }
            string readContents = "";
            
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                char lineType = ' ';
                string type;
                string array;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        lineType = line[0];
                        if (lineType == 'R')        // Receive port
                        {
                            type = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
                            array = Regex.Match(line, "\\[(\\d+)\\]").Groups[1].ToString();
                            indataLen += typeToLen(type, array);
                        }
                        else if (lineType == 'P')   // Provide port
                        {
                            type = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
                            array = Regex.Match(line, "[(.*)]").Groups[1].ToString();
                            outdataLen += typeToLen(type, array);
                        }
                        else if (lineType == 'N')   // Node Name
                        {
                            nodeName = Regex.Match(line, "\"(.*)\"").Groups[1].ToString();
                        }
                    }
                    readContents += line + "\n";
                }
            }
            readContents = readContents.Replace("\r", "\n");
            definition = readContents;

            if (nodeName.Length == 0)
            { throw new ArgumentException("nodeName string must not be empty"); }
            if (indataLen > 0)
            { createInPortDataFile(nodeName, indataLen); }
            else
            { throw new ArgumentException("indataLen not allowed to be 0"); }
            if (outdataLen > 0)
            { createOutPortDataFile(nodeName, outdataLen); }
            else
            { throw new ArgumentException("outdataLen not allowed to be 0"); }
            if (definition.Length > 0)
            { createDefinitionFile(nodeName, definition); }
            else
            { throw new ArgumentException("Definition string must not be empty"); }

        }
        
        private uint typeToLen(string type, string array = "")
        {
            int len = 0;
            switch (type)
            {
                case "a":
                    throw new NotImplementedException("TBD");
                case "c":
                    len = 1;
                    break;
                case "C":
                    len = 1;
                    break;
                case "s":
                    len = 2;
                    break;
                case "S":
                    len = 2;
                    break;
                case "l":
                    len = 4;
                    break;
                case "L":
                    len = 4;
                    break;
                case "u":
                    len = 4;
                    break;
                case "U":
                    len = 4;
                    break;
                default:
                    throw new ArgumentException("unhandled type");
            }
            if (array.Length > 0)
            {
                int val = int.Parse(array);
                if (val > 0)
                { len = val * len; } 
            }
            if (len <= 0)
            {
                throw new ArgumentException("len can't be 0");
            }

            return (uint)len;
        }

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
            inPortDataFile.setFileEventHandler(this);
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
            definitionFile.setFileEventHandler(this);
        }

    }
}
