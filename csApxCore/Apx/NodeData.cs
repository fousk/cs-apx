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

    // ToDO 
    // Duplicate Nodename = bad times, re-name automagically
    // Ports not processed according to APX doc (set up these as read port as well?)
    // Structs not handeled
    // Code("R"-line) is in a mess, clean up!

    public partial class NodeData : Apx.FileEventHandler
    {
        public Apx.File inPortDataFile;
        public Apx.File outPortDataFile;
        public Apx.File definitionFile;
        string nodeName;
        public List<ApxType> apxTypeList = new List<ApxType>();
        public List<ApxSignal> apxSignalList = new List<ApxSignal>();


        public NodeData(string path = "default")
        {
            nodeName = "csApxClient";
            uint indataLen = 0;
            uint outdataLen = 0;
            string definition = "";

            if (path == "default")
            { path = Apx.Constants.defaultDefinitionPath; }
            else if (path == "startupPath")
            {
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ApxDefinition.txt";
            }
            string readContents = "";
            
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                char lineType = ' ';
                string sigName;
                string type;
                string enumType;
                bool isArray = false;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        lineType = line[0];
                        if (lineType == 'T')        // Type
                        {   // R"PS_CabTiltLockWarning"C(0,7):=7
                            // "T[0]:=7 -> "C(0,7):=7
                            sigName = Regex.Match(line, "\"(.*?)\"").Groups[1].ToString();
                            // Should get something like "C(0,7)"
                            string arrTest = Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();
                            if (arrTest == "{")
                            { 
                                // Handle array types propperly later..
                                isArray = true; 
                            }
                            else
                            { isArray = false; }
                            type = Regex.Match(line, "\".*?\"(.+?\\))").Groups[1].ToString();

                            if (type == "") // Short type
                            {
                                type = Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();
                            }

                            addApxTypeToList(sigName, type, isArray);
                        }
                        else if (lineType == 'R')        // Receive port
                        {
                            sigName = Regex.Match(line, "\"(.*)\"").Groups[1].ToString();
                            Console.WriteLine(sigName);
                            enumType = Regex.Match(line, "\\[(\\d+)\\]").Groups[1].ToString();
                            if (enumType == "")
                            {
                                // "simple" type, no enum
                                type = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
                            }
                            else
                            {
                                // Indexed enum type (lookup in apxType)
                                type = apxTypeList[int.Parse(enumType)].typeDef;
                            }
                            if (type != "")
                            {
                                indataLen += typeToLen(type.Substring(0, 1), "");
                                //if (indataLen > 0)
                                //{
                                    addApxSignalToList(sigName, type, "");
                                //}
                            }
                            else
                            {
                                Console.WriteLine("type is empty");
                            }
                        }
                        else if (lineType == 'P')   // Provide port
                        {
                            type = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
                            enumType = Regex.Match(line, "[(.*)]").Groups[1].ToString();
                            outdataLen += typeToLen(type, "");
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

        public void onFileWrite(File file, uint offset, int dataLen)
        {
            Console.WriteLine("-- signal Update --");
            /*Console.WriteLine("File name: " + file.name);
            Console.WriteLine("NodeData: " + nodeName);
            Console.WriteLine("offset: " + offset);
            Console.WriteLine("data:" + BitConverter.ToString(data.ToArray()));
            */
            int parsed = 0;
            ApxSignal temp;
            string print;

            if (file.length >= (offset + (uint)dataLen))
            {
                while (parsed < dataLen)
                {
                    temp = apxSignalList[(int)offset + parsed];

                    print = "";
                    print += temp.name + " ";
                    print += typeToReadable(temp.type, 1, file.read((int)offset + parsed, (int)temp.len).ToArray());
                    Console.WriteLine(print);

                    if (temp.len > 0)
                    {
                        parsed += (int)temp.len;
                    }
                    else // APX file is corrupt
                    { break; }
                }
            }
            else
            { throw new ArgumentException("File write outside of memory"); }
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
