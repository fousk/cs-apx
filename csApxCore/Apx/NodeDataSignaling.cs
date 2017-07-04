using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Apx;

namespace Apx
{
    public class ApxSignal
    {
        public uint offset;
        public uint len;
        public char type;
        public string name;
        public ApxSignal(uint setOffset, uint setLen, char setType, string setName)
        {
            offset = setOffset;
            len = setLen;
            type = setType;
            name = setName;
        }
    }


    public class ApxType
    {
        public int minVal;
        public int maxVal;
        public bool isArray;
        public string sigName;
        public string typeName; // Not activly used except for debugging
        public List<string> structNames;
        public List<string> Defenitions;

        public ApxType(string sigName, string typeName, List<string> structNames, List<string> Defenitions, bool isArrayType = false)
        {
            this.sigName = sigName;
            this.typeName = typeName;
            this.structNames = structNames;
            this.Defenitions = Defenitions;
            this.isArray = isArrayType;
        }
    }
    

    public partial class NodeData : Apx.FileEventHandler
    {

        public void addApxTypeToList(string sigName, string typeName, List<string> structNames, List<string> typeIdentifiers, bool isArray, string interpretation = "")
        {
            ApxType atype = new ApxType(sigName, typeName, structNames, typeIdentifiers, isArray);
            apxTypeList.Add(atype);
        }

        //public void addApxSignalToList(string sigName, string type)
        public void addApxTypedSignalToList(ApxType aT)
        {
            uint offset = (uint)apxSignalList.Count;
            int arraySize = 0;
            for (int i = 0; i < aT.Defenitions.Count; i++)
            {
                string type = aT.Defenitions[i];
                string sigName = aT.sigName + ":" + aT.typeName;
                if (aT.structNames.Count > i)
                    sigName += ":" + aT.structNames[i];

                if (aT.isArray)
                    arraySize = aT.Defenitions.Count;

                uint typeLen = typeToLen(type, arraySize);

                indataLen += typeLen;
                for (int j = 0; j < typeLen; j++)
                {
                    // A 32bit signal is entered 4 times to make lookup easier as you can go by address anywhere in the range
                    // Handle arrays(ex string) as well
                    ApxSignal apxsig = new ApxSignal(offset, typeLen, type[0], sigName);
                    apxSignalList.Add(apxsig);
                }
            }
        }


        public string typeToReadable(char type, int array, byte[] data)
        {
            string res = "";

            if (array != 1 && type != 'a')
            { throw new NotImplementedException("arrays not handled, yet"); }

            switch (type)
            {
                case 'a':
                    res = "\"" + System.Text.Encoding.ASCII.GetString(data) + "\"";
                    break;
                case 'c':
                    res = ((int)(sbyte)data[0]).ToString();
                    break;
                case 'C':
                    res = ((uint)data[0]).ToString();
                    break;
                case 's':
                    res = BitConverter.ToInt16(data, 0).ToString();
                    break;
                case 'S':
                    res = BitConverter.ToUInt16(data, 0).ToString();
                    break;
                case 'l':
                    res = BitConverter.ToInt32(data, 0).ToString();
                    break;
                case 'L':
                    res = BitConverter.ToUInt32(data, 0).ToString();
                    break;
                case 'u':
                    res = BitConverter.ToInt64(data, 0).ToString();
                    break;
                case 'U':
                    res = BitConverter.ToUInt64(data, 0).ToString();
                    break;
                default:
                    throw new ArgumentException("unhandled type");
            }

            if (res.Length == 0)
            {
                throw new ArgumentException("No data parsed");
            }
            return res;
        }


        private uint typeToLen(char type, int arraySize = 0)
        { return typeToLen(type.ToString(), arraySize); }
        private uint typeToLen(string type, int arraySize = 0)
        {
            int len = 0;
            string t = type.Substring(0, 1); // C(0,1) -> C
            switch (t)
            {
                case "a": // Length of the stringis determined by the array parameter.
                    len = 1;
                    break;
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
            if (arraySize > 0)
            {
                len = arraySize * len;
            }
            if (len <= 0)
            {
                Console.WriteLine("Type T (unhandeled type)");
            }

            return (uint)len;
        }
    }
}
