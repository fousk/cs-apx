using System;

public static class NumHeader
{

    public static byte[] pack32(ulong inVal)
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
            ulong test = inVal >> 24;
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
    //return bytes([(0x80 | ( (value >> 24) & 0xFF)),(value >> 16) & 0xFF, (value >> 8) & 0xFF, (value & 0xFF) ])

    public static ulong unpack32(byte[] inArr)
    {
        if (inArr.Length == 1)
        {
            return (ulong)inArr[0];
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

            ulong res = BitConverter.ToUInt64(temp, 0);
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




