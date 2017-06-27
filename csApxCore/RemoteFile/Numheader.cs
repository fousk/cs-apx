using System;
using System.Collections.Generic;
public static class NumHeader
{

    public struct decodeReturn
    {
        public int bytesParsed;
        public uint value;

    }


    public static decodeReturn decode(List<byte> data, int offset, int mode = 32)
    {
        decodeReturn ret = new decodeReturn();
        uint value;
        int end = data.Count;
        byte b1;

        if (offset+1 <= data.Count)
        {
            b1 = data[offset];
            if ((b1 & 0x80) == 0x80)
            {
                if (mode == 32)
                {
                    if (offset+4 <= data.Count)
                    {
                        List<byte> val = data.GetRange(offset, offset + 4);
                        val.Reverse();  // From Big endian to Little endian
                        value = BitConverter.ToUInt32(val.ToArray(), 0);
                        ret.value = (value & 0x7FFFFFFF);
                        ret.bytesParsed = 4;
                    }
                    else
                    { throw new ArgumentException("data field not long enough"); }
                }
                else
                { throw new ArgumentException("Only 8 & 32b supported");  }
            }
            else 
            { 
                ret.value = b1;
                ret.bytesParsed = 1;
            }
        }
        else
        { throw new ArgumentException("data field not long enough"); }

        return ret;
    }


    public static List<byte> encode(uint value, int mode = 32)
    {
        List<byte> data = new List<byte>();
        
        if (value <= 127)
        {
            data.Add((byte)value);
        }
        else
        {
            if (mode == 32)
            {
                if (value <= 0x7FFFFFFF)
                {
                    data.AddRange(BitConverter.GetBytes(value));
                    data.Reverse();
                    data[0] |= 0x80;
                }
                else
                { throw new ArgumentException("value over 0x7FFFFFFF"); }
            }
            else
            { throw new ArgumentException("only 8/32b mode supported"); }
        }

        return data;
    }


    public static byte[] pack32(uint inVal)
    {
        if (inVal < 128)
        {
            byte[] parsed = new byte[1];
            parsed[0] = BitConverter.GetBytes(inVal)[0];
            return parsed;
        }
        else if (inVal < 2147483648)
        {
            byte[] res = BitConverter.GetBytes(inVal);
            //bytes([(0x80 | ( (value >> 24) & 0xFF)),(value >> 16) & 0xFF, (value >> 8) & 0xFF, (value & 0xFF) ])
            byte[] parsed = new byte[4];
            uint test = inVal >> 24;
            parsed[0] = BitConverter.GetBytes(0x80 | (inVal >> 24) & 0xFF)[0];
            parsed[1] = BitConverter.GetBytes((inVal >> 16) & 0xFF)[0];
            parsed[2] = BitConverter.GetBytes((inVal >> 8) & 0xFF)[0];
            parsed[3] = BitConverter.GetBytes(inVal & 0xFF)[0];
            return parsed;
        }
        else
        {
            throw new System.ArgumentException("input value '" + inVal + "' out of range");
        }
    }


    public static uint unpack32(byte[] inArr)
    {
        if (inArr.Length == 1)
        {
            return (uint)inArr[0];
        }
        else if (inArr.Length == 4)
        {
            // BitConverter.ToUInt64 requires 8 bytes in...
            // Also has another byte order
            byte[] temp = new byte[8];
            temp[3] = inArr[0];
            temp[2] = inArr[1];
            temp[1] = inArr[2];
            temp[0] = inArr[3];

            uint res = BitConverter.ToUInt32(temp, 0);
            if (res >= 2147483648)
                return res - 2147483648;
            else
            {
                throw new System.ArgumentException("Invalid input");
            }
        }
        else
        {
            throw new System.ArgumentException("input must be one or four bytes long!");
        }
    }
}




