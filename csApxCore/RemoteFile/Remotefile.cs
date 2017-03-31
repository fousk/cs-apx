using System;
namespace RemoteFile
{

    public static class Constants
    {
        public const ulong RMF_CMD_START_ADDR = 0x3FFFFC00;

        public const int RMF_FILE_TYPE_FIXED = 0;
        public const int RMF_FILE_TYPE_DYNAMIC = 1;
        public const int RMF_FILE_TYPE_STREAM = 2;

        public const int RMF_CMD_ACK = 0;    //reserved for future use
        public const int RMF_CMD_NACK = 1;    //reserved for future use
        public const int RMF_CMD_EOT = 2;    //reserved for future use
        public const int RMF_CMD_FILE_INFO = 3;
        public const int RMF_CMD_FILE_OPEN = 10;
        public const int RMF_CMD_FILE_CLOSE = 11;

        public const int RMF_DIGEST_TYPE_NONE = 0;

        public const int RMF_MSG_CONNECT = 0;
        public const int RMF_MSG_FILEINFO = 1;
        public const int RMF_MSG_FILEOPEN = 2;
        public const int RMF_MSG_FILECLOSE = 3;
        public const int RMF_MSG_WRITE_DATA = 4;

        public const int RMF_FILEINFO_BASE_LEN = 48;
    }

    public class File
    {
        public string name {get; set;}
        public int length {get; set;}
        public int fileType {get; set;}
        public ulong address {get; set;}
        public int digestType {get; set;}
        public byte[] digestData {get; set;}
        public bool isRemoteFile {get; set;}
        public bool isOpen {get; set;}

        public File()
        {
            digestType = Constants.RMF_DIGEST_TYPE_NONE;
            digestData = new byte[32];
            isRemoteFile = false;
            isOpen = false;
        }

        public void open()
        {
            isOpen = true;
        }

        public void close()
        {
            isOpen = false;
        }
    }

    public interface FileMap
    {
        bool insert(File file);
        bool remove(File file);
        bool assignFileAddressDefault(File file);
    }
    
}


public static class RemoteFileUtil
{
    public static byte[] packHeader(ulong address, bool more_bit)
    {
        if (address < 16384)
        {
            byte[] res = new byte[2] { oneByte((address >> 8) & 0x3F), oneByte(address & 0xFF) };
            if (more_bit)
            {
                res[0] = (byte)((int)res[0] | 0x40);
            }
            return res;

        }
        else if (address < 1073741824)
        {
            byte[] res = new byte[4] { (byte)((address >> 24) & 0x3F | 0x80), (byte)((address >> 16) & 0xFF), (byte)((address >> 8)), (byte)((address) & 0xFF) };
            if (more_bit)
            {
                res[0] = (byte)((int)res[0] | 0x40);
            }
            return res;
        }
        else
        {
            throw new System.ArgumentException("input value '" + address + "' out of range");
        }
    }

    public struct headerReturn
    {
        public int bytes_parsed;
        public ulong address;
        public bool more_bit;
    }
    public static headerReturn unpackHeader(byte[] data)
    {
        headerReturn ret = new headerReturn();
        int i = 0;
        bool more_bit = false;
        ulong address = ulong.MaxValue;
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
                    address = (ulong)((b0 << 24) + (b1 << 16) + (b2 << 8) + b3);
                }
            }
            else
            {
                b0 = b0 & 0x3F;     // Remove more_bit for address conversion
                address = (ulong)((b0 << 8) + b1);
            }
        }
        if (address != ulong.MaxValue)
        {
            ret.address = address;
            ret.bytes_parsed = i;
            ret.more_bit = more_bit;
        }
        else    // Invalid header
        {
            ret.address = ulong.MaxValue;
            ret.bytes_parsed = 0;
            ret.more_bit = false;
        }
        return ret;
    }


    public static byte oneByte(ulong val)
    {
        byte res = BitConverter.GetBytes(val)[0];
        return res;
    }
}

