using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Apx;
using System.Collections.Concurrent;

namespace Apx
{

    // ToDO 
    // Provide ports not processed according to APX doc (all is set as read for the moment)

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

        ConcurrentQueue<ExternalMsg> externalQueue;

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

        public void setExternalQueue(ConcurrentQueue<ExternalMsg> setExternalQueue)
        {
            externalQueue = setExternalQueue;
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
                {
                    // Hack to listen to both Receive and Provide ports fom a given APX file.
                    line = 'R' + line.Substring(1);
                    addReceiveportToList();
                }
                else if (apxLineType == 'N')   // Node Name
                    replaceApxDefenitionNodeName();
                else if (apxLineType == '#')             // Commented out
                    Console.WriteLine("skipping row: " + line);
            }
            if (apxLineType != '#' || line.Length == 0)
                readContents += line + "\n";
        }

        private void addDummyProvidePort()
        {
            // There is a bug in ApxServer which requires atleast one Provideport to function, so we create a dummyport.
            string dummyLine = "P\"dummyProvidePort\"C";
            string psigType = Regex.Match(dummyLine, "\".*\"(.)").Groups[1].ToString();
            outdataLen += typeToLen(psigType);
        }

        private void replaceApxDefenitionNodeName()
        {
            line = "N\"" + nodeName + "\"";
        }

        private void addReceiveportToList()
        {
            string sigName = getStringWithinQuotes();
            string signalType = getSignalType();
            string typeIdentifier = getnumericalInHardBrackets();

            ApxType aT;
            int index = int.MaxValue;
            if (typeIdentifier != "")
                index = int.Parse(typeIdentifier);

            if (signalType == "T")
            {
                if (apxTypeList.Count > index)
                {
                    aT = apxTypeList[index];
                    aT.sigName = sigName;
                }
                else
                    throw new ArgumentException("not able to parse line to ApxType");
            }
            else if (signalType != "")
            {
                List<string> names = new List<string> {""};
                List<string> defenitions;
                if (index == int.MaxValue)
                    defenitions = new List<string> {signalType};
                else
                {
                    defenitions = new List<string>();
                    defenitions.Add(signalType + "[" + index.ToString() + "]");
                }

                aT = new ApxType(sigName, signalType, names, defenitions);
            }
            else
                throw new ArgumentException("not able to parse line to ApxType");

            if (aT != null)
                addApxTypedSignalToList(aT);
            else
                throw new ArgumentException("not able to parse line to ApxType");
        }

        private string getSignalType()
        {
            return Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();;
        }

        private string getStringWithinQuotes()
        {
            return Regex.Match(line, "\"(.*?)\"").Groups[1].ToString();
        }
        
        private string getsignalTypefromTypeIdentifier(string enumType)
        {
            string st;
            if (enumType == "")     // "simple" type, no Type reference
                st = Regex.Match(line, "\".*\"(.)").Groups[1].ToString();
            else
                st = apxTypeList[int.Parse(enumType)].Defenitions[0];
            return st;
        }

        private string getnumericalInHardBrackets()
        { return getnumericalInHardBrackets(line); }
        
        private string getnumericalInHardBrackets(string input)
        {
            return Regex.Match(input, "\\[(\\d+)\\]").Groups[1].ToString();
        }

        private void addTypeDefenitionToList()
        {
            bool isStruct = isTypeWithStructData();
            string typeName = Regex.Match(line, "\"(.*?)\"").Groups[1].ToString();
            if (isStruct)
                addApxStructTypeToList(typeName);
            else
                addApxSingleTypeToList(typeName);
        }

        private void addApxSingleTypeToList(string typeName)
        {
            string typeIdentifier = Regex.Match(line, "\".*?\"(.+?\\))").Groups[1].ToString();
            string arrayString = getnumericalInHardBrackets();
            List<string> tmp = new List<string>();
            int arraySize = int.MaxValue;

            if (arrayString != "")
                arraySize = int.Parse(arrayString);

            if (typeIdentifier == "") // Short type
                typeIdentifier = Regex.Match(line, "\".*?\"(.)").Groups[1].ToString();
            if (arraySize == int.MaxValue)
                tmp.Add(typeIdentifier); // new List<string>();
            else
            {
                tmp.Add(typeIdentifier + "[" + arraySize + "]");
            }

            addApxTypeToList("", typeName, new List<string>(), tmp);
        }

        private void addApxStructTypeToList(string typeName)
        {
            List<string> types = new List<string>();
            List<string> names = new List<string>();
            MatchCollection structNames = Regex.Matches(line, "\"(.*?)\"");
            foreach (Match nameMatch in structNames)
            {
                string structName = nameMatch.Groups[1].ToString();
                string pattern = structName + "\"([^:\"$}]+)";
                MatchCollection typeMatches = Regex.Matches(line, pattern);
                if (typeMatches.Count > 0)
                {
                    string structType = typeMatches[0].Groups[1].ToString();

                    if (structType != "{")    // Ignore First match
                    {
                        names.Add(structName);
                        types.Add(structType);
                    }
                }
                else
                    throw new ArgumentException("Following line not decoded: " + line);
            }
            addApxTypeToList("", typeName, names, types);
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
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ApxDefinition.apx";
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
            ApxSignal aS;
            string print;

            if (file.length >= (offset + (uint)dataLen))
            {
                while (parsed < dataLen)
                {
                    aS = apxSignalList[(int)offset + parsed];

                    if (externalQueue != null)
                    {
                        externalQueue.Enqueue(new ExternalMsg(aS.name, typeToReadable(aS.type, 1, file.read((int)offset + parsed, (int)aS.len).ToArray())));
                    }
                    else
                    {
                        print = "";
                        print += aS.name + " ";
                        print += typeToReadable(aS.type, 1, file.read((int)offset + parsed, (int)aS.len).ToArray());
                        Console.WriteLine(print);
                    }

                    if (aS.len > 0)
                    {
                        parsed += (int)aS.len;
                    }
                    else // APX file is corrupt
                    { break; }
                }
            }
            else
                throw new ArgumentException("File write outside of memory");
        }

    }
}
