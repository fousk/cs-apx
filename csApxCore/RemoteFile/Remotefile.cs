using System;
using System.Text;
using System.Collections.Generic;
using RemoteFile;


namespace RemoteFile
{

    public static class Constants
    {
        public const uint RMF_CMD_START_ADDR = 0x3FFFFC00;

        public const uint RMF_FILE_TYPE_FIXED = 0;
        public const uint RMF_FILE_TYPE_DYNAMIC = 1;
        public const uint RMF_FILE_TYPE_STREAM = 2;

        public const uint RMF_CMD_ACK = 0;    //reserved for future use
        public const uint RMF_CMD_NACK = 1;    //reserved for future use
        public const uint RMF_CMD_EOT = 2;    //reserved for future use
        public const uint RMF_CMD_FILE_INFO = 3;
        public const uint RMF_CMD_FILE_HEARTBEAT_REQ = 5;
        public const uint RMF_CMD_FILE_HEARTBEAT_RESP = 6;
        public const uint RMF_CMD_FILE_PING_REQ = 7;
        public const uint RMF_CMD_FILE_PING_RESP = 8;
        public const uint RMF_CMD_FILE_OPEN = 10;
        public const uint RMF_CMD_FILE_CLOSE = 11;

        public const uint RMF_DIGEST_TYPE_NONE = 0;

        public const uint RMF_MSG_CONNECT = 0;
        public const uint RMF_MSG_FILEINFO = 1;
        public const uint RMF_MSG_FILEOPEN = 2;
        public const uint RMF_MSG_FILECLOSE = 3;
        public const uint RMF_MSG_WRITE_DATA = 4;

        public const uint RMF_FILEINFO_BASE_LEN = 48;
    }
    
}


public static class RemoteFileUtil
{
    public static List<byte> packHeader(uint address, bool more_bit = false)
    {
        List<byte> ret = new List<byte>();
        if (address < 16384)
        {
            byte[] res = new byte[2] { oneByte((address >> 8) & 0x3F), oneByte(address & 0xFF) };
            if (more_bit)
            {
                res[0] = (byte)((int)res[0] | 0x40);
            }
            ret.AddRange(res);
        }
        else if (address < 1073741824)
        {
            byte[] res = new byte[4] { (byte)((address >> 24) & 0x3F | 0x80), (byte)((address >> 16) & 0xFF), (byte)((address >> 8)), (byte)((address) & 0xFF) };
            if (more_bit)
            {
                res[0] = (byte)((int)res[0] | 0x40);
            }
            ret.AddRange(res);
        }
        else
        {
            throw new System.ArgumentException("input value '" + address + "' out of range");
        }
        return ret;
    }


    public struct headerReturn
    {
        public int bytes_parsed;
        public uint address;
        public bool more_bit;
    }


    public static headerReturn unpackHeader(byte[] data)
    {
        headerReturn ret = new headerReturn();
        int i = 0;
        bool more_bit = false;
        uint address = uint.MaxValue;
        int b0, b1, b2, b3;

        if (data.Length >= 2)   // at least 2B data
        {
            b0 = data[0];
            b1 = data[1];
            i = 2;
            if ((b0 & 0x40) == 0x40) { more_bit = true; }
            if ((b0 & 0x80) == 0x80)   // High bit is set, 4B header
            {
                if (data.Length >= 4)   // at least 4B data
                {
                    b0 = b0 & 0x3F;     // Remove more_bit for address conversion
                    b2 = data[2];
                    b3 = data[3];
                    i = 4;
                    address = (uint)((b0 << 24) + (b1 << 16) + (b2 << 8) + b3);
                }
            }
            else
            {
                b0 = b0 & 0x3F;     // Remove more_bit for address conversion
                address = (uint)((b0 << 8) + b1);
            }
        }
        if (address != uint.MaxValue)
        {
            ret.address = address;
            ret.bytes_parsed = i;
            ret.more_bit = more_bit;
        }
        else    // Invalid header
        {
            ret.address = uint.MaxValue;
            ret.bytes_parsed = 0;
            ret.more_bit = false;
        }
        return ret;
    }


    public static List<byte> packFileOpen(uint address, string byteOrder = "<")
    {
        List<byte> blist = new List<byte>();
        if ((byteOrder == "<")) // Only little endian supportet at the moment
        {
            blist.AddRange(BitConverter.GetBytes(Constants.RMF_CMD_FILE_OPEN));
            blist.AddRange(BitConverter.GetBytes(address));
        }
        else
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        return blist;
    }


    public static uint unPackFileOpen(List<byte> data, string byteOrder = "<")
    {
        uint address;
        uint cmd_type;
        if ((byteOrder == "<")) // Only little endian supportet at the moment
        {
            //Expected commandtype and address
            if (data.Count == 8)
            {
                cmd_type = BitConverter.ToUInt32(data.GetRange(0,4).ToArray(), 0);
                address = BitConverter.ToUInt32(data.GetRange(4, 4).ToArray(), 0);
                if (cmd_type != Constants.RMF_CMD_FILE_OPEN)
                { throw new ArgumentException("Expected commandtype == RMF_CMD_FILE_OPEN"); }
            }
            else
            { throw new ArgumentException("Expected commandtype and address, 8bytes"); }
        }
        else
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        return address;
    }


    public static List<byte> packFileClose(uint address, string byteOrder = "<")
    {
        List<byte> blist = new List<byte>();
        if ((byteOrder == "<")) // Only little endian supportet at the moment
        {
            blist.AddRange(BitConverter.GetBytes(Constants.RMF_CMD_FILE_CLOSE));
            blist.AddRange(BitConverter.GetBytes(address));
        }
        else
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        return blist;
    }


    public static uint unPackFileClose(List<byte> data, string byteOrder = "<")
    {
        uint address;
        uint cmd_type;
        if ((byteOrder == "<")) // Only little endian supportet at the moment
        {
            //Expected commandtype and address
            if (data.Count == 8)
            {
                cmd_type = BitConverter.ToUInt32(data.GetRange(0,4).ToArray(), 0);
                address = BitConverter.ToUInt32(data.GetRange(4, 4).ToArray(), 0);
                if (cmd_type != Constants.RMF_CMD_FILE_CLOSE)
                { throw new ArgumentException("Expected commandtype == RMF_CMD_FILE_CLOSE"); }
            }
            else
            { throw new ArgumentException("Expected commandtype and address, 8bytes"); }
        }
        else
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        return address;
    }


    public static List<byte> packFileInfo(File file, string byteOrder = "<")
    {
        if ((byteOrder != "<")) // Only little endian supportet at the moment
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        if (file.address == uint.MaxValue)
        {throw new ArgumentException("file doesn't contain an initialized address");}
        else
        {
            List<byte> blist = new List<byte>();
            blist.AddRange(BitConverter.GetBytes(Constants.RMF_CMD_FILE_INFO));
            blist.AddRange(BitConverter.GetBytes(file.address));
            blist.AddRange(BitConverter.GetBytes(file.length));
            blist.AddRange(BitConverter.GetBytes((UInt16)file.fileType));
            blist.AddRange(BitConverter.GetBytes((UInt16)file.digestType));
            blist.AddRange(file.digestData);
            blist.AddRange(ASCIIEncoding.ASCII.GetBytes(file.name));
            blist.Add(0); // null-termination
                
            return blist;
        }
    }


    public static File unPackFileInfo(List<byte> data, string byteOrder = "<")
    {
        if ((byteOrder != "<")) // Only little endian supportet at the moment
        { throw new ArgumentException("only byteorder '<' (Little Endian) is supported"); }

        File file = new File();
        int baseLen = (int)Constants.RMF_FILEINFO_BASE_LEN;
        if (data.Capacity >= baseLen)
        {
            uint cmdType = BitConverter.ToUInt32(data.GetRange(0,4).ToArray(), 0);
            uint address = BitConverter.ToUInt32(data.GetRange(4,4).ToArray(), 0);
            uint length = BitConverter.ToUInt32(data.GetRange(8,4).ToArray(), 0);
            uint fileType = BitConverter.ToUInt16(data.GetRange(12,2).ToArray(), 0);
            uint digestType = BitConverter.ToUInt16(data.GetRange(14,2).ToArray(), 0);
            List<byte> digestData = data.GetRange(16,32);

            List<byte> bname = data.GetRange(baseLen, data.Count- baseLen -1);
            string name = System.Text.Encoding.ASCII.GetString(bname.ToArray());

            file.address = address;
            file.length = length;
            file.fileType = fileType;
            file.digestType = digestType;
            file.digestData = digestData.ToArray();
            file.name = name;
        }

        return file;
    }


    public static byte oneByte(uint val)
    {
        byte res = BitConverter.GetBytes(val)[0];
        return res;
    }
}

