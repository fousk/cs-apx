using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apx
{

    public interface FileEventHandler   //NodeDataHandler in other languages
    {
        void onFileWrite(File file, uint offset, int dataLen);
    }


    public class File : RemoteFile.File
    {
        private Object thisLock = new Object();
        private Byte[] data;
        private FileEventHandler fileEventHandler;

        public File(string inName, uint inLength) : base(inName, inLength)
        {
            data = new byte[inLength];
            length = inLength;
        }

        public List<byte> read(int offset, int len)
        {
            List<byte> list = new List<byte>();

            if ((offset < 0) || (offset + len > length) || (offset + len > data.Length))
            {
                throw new ArgumentException("file read outside file boundary detected");
            }
            else
            {
                lock (thisLock)
                {
                    list.AddRange(data.Skip(offset).Take(len).ToArray());
                }
            }
            return list;
        }


        public int write(uint offset, List<byte> inData, bool moreBit = false)
        {
            int len = 0;

            if ((offset < 0) || (offset + inData.Count) > length || (offset + inData.Count > data.Length))
            {
                throw new ArgumentException("file write outside file boundary detected");
            }
            else
            {
                lock (thisLock)
                {
                    for (int i = 0; i < inData.Count; i++)
                    { data[offset + i] = inData[i]; }
                    len = inData.Count;
                }
                if (fileEventHandler != null)
                {
                    fileEventHandler.onFileWrite(this, offset, inData.Count);
                }
            }
            return len;
        }


        public void setFileEventHandler(FileEventHandler eventHandler)
        {
            fileEventHandler = eventHandler;
        }
    }
}
