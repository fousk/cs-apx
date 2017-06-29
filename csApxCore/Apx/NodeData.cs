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
    // Ports not processed according to APX doc (set up these as read port as well?)
    // Structs not handeled
    // Code("R"-line) is in a mess, clean up!

    public partial class NodeData : Apx.FileEventHandler
    {
        public Apx.File inPortDataFile;
        public Apx.File outPortDataFile;
        public Apx.File definitionFile;
        public List<ApxType> apxTypeList = new List<ApxType>();
        public List<ApxSignal> apxSignalList = new List<ApxSignal>();
        string nodeName = "CSApxClient";
        uint indataLen = 0;
        uint outdataLen = 0;
        string line;
        char apxLineType = ' ';
        string path;
        string readContents = "";

        public NodeData(string inPath = "default")
        {
            path = setPathToApxFile(inPath);
            
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    processApxDefenitionLine();
                }
                addDummyProvidePort();
            }
            readContents = readContents.Replace("\r", "\n");

            createApxFiles();
        }

        private void processApxDefenitionLine()
        {
            if (line.Length > 0)
            {
                apxLineType = line[0];
                if (apxLineType == 'T')        // Signal type
                    addTypeDefenitionToList();
                else if (apxLineType == 'R')        // Receive port
                    addReceiveportToList();
                else if (apxLineType == 'P')        // Provide port
                    addReceiveportToList();         // Listen to all provide ports as well
                else if (apxLineType == 'N')   // Node Name
                    replaceApxDefenitionNodeName();
            }
            readContents += line + "\n";
        }

        private void addDummyProvidePort()
        {
            // There is a bug in ApxServer which requires atleast one Provideport to function, so we create a dummyport.
            string dummyLine = "P\"dummyProvidePort\"C";
            string psigType = Regex.Match(dummyLine, "\".*\"(.)").Groups[1].ToString();
            outdataLen += typeToLen(psigType, "");
        }

        private void replaceApxDefenitionNodeName()
        {
            line = "N\"" + nodeName + "\"";
        }

        private void addReceiveportToList()
        {
            string sigName = getStringWithinParentheses();
            string sigType = getSignalTypeDefinition();
            parseAndAddApxSignalToList(sigName, sigType);
        }

        private void parseAndAddApxSignalToList(string sigName, string sigType)
        {
            if (sigType != "")
            {
                indataLen += typeToLen(sigType.Substring(0, 1), "");
                addApxSignalToList(sigName, sigType);
            }
            else
                Console.WriteLine("type is empty");
        }

        private string getStringWithinParentheses()
        {
            return Regex.Match(line, "\"(.*)\"").Groups[1].ToString();
        }

        private string getSignalTypeDefinition()
        {
            string typeIdentifier = getnumericalInHardBrackets();
            string st = getsignalTypefromTypeIdentifier(typeIdentifier);
            return st;
        }

        private string getsignalTypefromTypeIdentifier(string enumType)
        {
            string st;
            if (enumType == "")     // "simple" type, no Type reference
                st = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
            else
                st = apxTypeList[int.Parse(enumType)].typeDef;
            return st;
        }

        private string getnumericalInHardBrackets()
        {
            string enumType = Regex.Match(line, "\\[(\\d+)\\]").Groups[1].ToString();
            return enumType;
        }

        private void addTypeDefenitionToList()
        {
            bool isStruct = isTypeWithStructData();
            string typeName = Regex.Match(line, "\"(.*?)\"").Groups[1].ToString();
            if (isStruct)
            {
                MatchCollection matches = Regex.Matches(line, "\"(.*?)\"");
                //string tst = matches[0].Groups[1].ToString();
                List<string> lst = new List<string>();
                foreach (Match match in matches)
                {
                    string structName = match.Groups[1].ToString();
                    string pattern = structName + "\"([^:\"$}]+)";
                    MatchCollection m = Regex.Matches(line, pattern);
                    if (m.Count > 0)
                    {
                        string type = m[0].Groups[1].ToString();
                        if (type != "{")    // Ignore First match
                            lst.Add(type);
                    }
                    else
                        throw new ArgumentException("Following line not decoded: " + line );
                }
                addApxTypeToList(typeName, lst.ToString(), isStruct);
            }
            else
            {
                string typeIdentifier = Regex.Match(line, "\".*?\"(.+?\\))").Groups[1].ToString();

                if (typeIdentifier == "") // Short type
                    typeIdentifier = Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();
                addApxTypeToList(typeName, typeIdentifier, isStruct);
            }

        }

        private bool isTypeWithStructData()
        {
            string arrTest = Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();
            if (arrTest == "{")
                return true;
            else
                return false; 
        }

        private static string setPathToApxFile(string path)
        {
            if (path == "default")
            { path = Apx.Constants.defaultDefinitionPath; }
            else if (path == "startupPath")
            {
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ApxDefinition.txt";
            }
            return path;
        }

        private void createApxFiles()
        {
            if (nodeName.Length == 0)
                throw new ArgumentException("nodeName string must not be empty");

            createInPortDataFile(nodeName, indataLen);
            createOutPortDataFile(nodeName, outdataLen);
            createDefinitionFile(nodeName, readContents);
        }

        private void createInPortDataFile(string nodeName, uint indataLen)
        {
            if (indataLen > 0)
            {
                inPortDataFile = new Apx.File(nodeName + ".in", indataLen);
                inPortDataFile.setFileEventHandler(this);
            }
            else
                throw new ArgumentException("indataLen not allowed to be 0");
        }


        private void createOutPortDataFile(string nodeName, uint outDataLen)
        {
            if (outdataLen > 0)
                outPortDataFile = new Apx.File(nodeName + ".out", outDataLen);
            else
                throw new ArgumentException("outdataLen not allowed to be 0");
        }


        private void createDefinitionFile(string nodeName, string definition)
        {
            if (readContents.Length > 0)
            {
                List<Byte> definitionBytes = new List<byte>();
                definitionBytes.AddRange(ASCIIEncoding.ASCII.GetBytes(definition));
                definitionFile = new Apx.File(nodeName + ".apx", (uint)definitionBytes.Count);
                definitionFile.write(0, definitionBytes);
                Console.WriteLine("definitionFile length: " + definitionFile.length);
                definitionFile.setFileEventHandler(this);
            }
            else
                throw new ArgumentException("Definition string must not be empty");
        }

        public void onFileWrite(File file, uint offset, int dataLen)
        {
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

    }
}
