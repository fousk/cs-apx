using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Apx
{
    class FileManager : RemoteFile.FileManager
    {
        public FileManager(FileMap localFileMap, FileMap remoteFileMap) : base(localFileMap, localFileMap)
        {

        }

        public void attachNodeData()
        {
            throw new System.NotImplementedException();
        }
    }
}
