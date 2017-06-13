using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFile
{
    public class File
    {
        public string name { get; set; }
        public uint length { get; set; }
        public uint fileType { get; set; }
        public uint address { get; set; }
        public uint digestType { get; set; }
        public byte[] digestData { get; set; }
        public bool isRemoteFile { get; set; }
        public bool isOpen { get; set; }

        public File()
        {
            digestType = Constants.RMF_DIGEST_TYPE_NONE;
            digestData = new byte[32];
            isRemoteFile = false;
            isOpen = false;
        }

        public File(string inName, uint inLength)
        {
            digestType = Constants.RMF_DIGEST_TYPE_NONE;
            digestData = new byte[32];
            isRemoteFile = false;
            isOpen = false;
            name = inName;
            length = inLength;
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
