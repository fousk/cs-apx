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

        public List<byte> read(int offset, int len)
        {
            List<byte> list = new List<byte>();

            if ((offset < 0) || (offset + len > length))
            {
                throw new ArgumentException("file read outside file boundary detected");
            }
            else
            {
                lock (thisLock)
                {
                    list.AddRange(digestData.Skip(offset).Take(len).ToArray());
                }
            }
            return list;
        }

        public int write(int offset, List<byte> data)
        {
            int len = 0;

            if ((offset < 0) || (offset + data.Count) > length)
            {
                throw new ArgumentException("file write outside file boundary detected");
            }
            else
            {
                lock (thisLock)
                {
                    for (int i = 0; i < data.Count; i++)
                    { digestData[offset + i] = data[i]; }
                    len = data.Count;
                }
            }
            return len;
        }
    }
}
