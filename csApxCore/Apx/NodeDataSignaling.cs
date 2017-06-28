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
        public bool isStruct;
        public int structLen;
        public string typeDef;
        public string typeName; // Not activly used except for debugging
        public ApxType(string name, string type, bool isStructType= false, int structLength = 0)
        {
            typeName = name;
            typeDef = type;
            isStruct = isStructType;
            structLen = structLength;
        }
    }
    

    public partial class NodeData : Apx.FileEventHandler
    {
        public string getApxType(string index)
        { return getApxType(int.Parse(index)); }
        public string getApxType(int index)
        {
            string Type = "";

            return Type;
        }


        public void addApxTypeToList(string typeName, string typeIdentifier, bool isStruct, string interpretation = "")
        {
            ApxType atype = new ApxType(typeName, typeIdentifier, isStruct);
            apxTypeList.Add(atype);
        }

        
        public void addApxSignalToList(string sigName, string type)
        {
            // Redo Array/arr section...
            string array = "";
            int arr;
            if (array.Length > 0)
            { arr = int.Parse(array); }
            else
            { arr = 1; }
            uint offset = (uint)apxSignalList.Count;
            for (int i = 0; i < arr; i++)
            {
                uint typeLen = typeToLen(type);
                for (int j = 0; j < typeLen; j++)
                {
                    // A 32bit signal is entered 4 times to make lookup easier as you can go by address anywhere in the range
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
                    res = System.Text.Encoding.ASCII.GetString(data);
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


        private uint typeToLen(char type, string array = "")
        { return typeToLen(type.ToString(), array); }
        private uint typeToLen(string type, string array = "")
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
                case "T":
                    len = 0;
                    break;
                case "{":
                    len = 0;
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
                Console.WriteLine("Type T (unhandeled type)");
            }

            return (uint)len;
        }
    }
}
