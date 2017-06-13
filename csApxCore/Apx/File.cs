using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apx
{
    public class File : RemoteFile.File
    {
        private Object thisLock = new Object();
        private Byte[] data;

        public File(string inName, uint inLength) : base(inName, inLength)
        {
            data = new byte[inLength];
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

        public int write(int offset, List<byte> inData)
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
                    { data[offset + i] = data[i]; }
                    len = inData.Count;
                }
            }
            return len;
        }
    }
}
