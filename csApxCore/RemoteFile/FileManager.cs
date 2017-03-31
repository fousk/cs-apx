using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;


namespace RemoteFile
{
    class FileManager 
    {
        public FileMap localFileMap, remoteFileMap;
        public List<File> requestedFiles = new List<File>();
        public string byteOrder = "<"; // use '<' for little endian, '>' for big endian
        public ConcurrentQueue<Msg> msgQueue = new ConcurrentQueue<Msg>();

        Thread workerThread = new Thread(new ThreadStart(worker));


        public FileManager(FileMap setLocalFileMap, FileMap setRemoteFileMap)
        {
            localFileMap = setLocalFileMap;
            remoteFileMap = setRemoteFileMap;

            workerThread.Name = "worker";
            workerThread.Start();
        }

        public void worker()
        {
            Msg msg = new Msg();
            while (true)
            {
                msgQueue.TryDequeue(out msg);



                throw new System.NotImplementedException();
            }
        }
        

        public void attachLocalFile(File file)
        {
            localFileMap.insert(file);
        }

        public void attachRemoteFile(File file)
        {
            remoteFileMap.insert(file);
        }

    }


}
