using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace RemoteFile
{
    public class Client
    {

        SocketAdapter socketAdapter = new SocketAdapter();
        FileManager fileManager = new FileManager();
        //socketAdapter.setRecieveHandler(fileManager);
        Client()
        {
            socketAdapter.setRecieveHandler(fileManager);
        }
    }
    
    public class ReceiveHandler
    {
        public virtual void onConnected(TransmitHandler handler)
        {

        }
    }
    public class FileManager : ReceiveHandler
    {
        public FileMap localFileMap, remoteFileMap;
        public List<File> requestedFiles = new List<File>();
        public string byteOrder = "<"; // use '<' for little endian, '>' for big endian

        TransmitHandler transmitHandler;

        EventWaitHandle waitHandle = new AutoResetEvent(false);
        ConcurrentQueue<Msg> msgQueue = new ConcurrentQueue<Msg>();
        
        /*public FileManager(FileMap setLocalFileMap, FileMap setRemoteFileMap)
        {
            localFileMap = setLocalFileMap;
            remoteFileMap = setRemoteFileMap;
        }*/

        public override void onConnected(TransmitHandler handler)
        {
            transmitHandler = handler;
        }

        void worker()
        {
            Msg msg = new Msg();
            while (true)
            {
                msgQueue.TryDequeue(out msg);



                throw new System.NotImplementedException();
            }
        }

        public void Enqueue(Msg msg)
        {
            msgQueue.Enqueue(msg);
            waitHandle.Set();
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
