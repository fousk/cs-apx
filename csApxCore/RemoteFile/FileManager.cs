using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using RemoteFile;

namespace RemoteFile
{
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
        public Guid InstanceID { get; private set; }    // Check that we use the right instance

        public TransmitHandler transmitHandler;

        EventWaitHandle waitHandle = new AutoResetEvent(false);
        ConcurrentQueue<Msg> msgQueue = new ConcurrentQueue<Msg>();

        public FileManager()
        {
            this.InstanceID = Guid.NewGuid();
        }

        /*public FileManager(FileMap setLocalFileMap, FileMap setRemoteFileMap)
        {
            localFileMap = setLocalFileMap;
            remoteFileMap = setRemoteFileMap;
        }*/

        public override void onConnected(TransmitHandler handler)
        {
            transmitHandler = handler;
        }

        public void worker()
        {
            Msg msg;
            while (true)
            {
                msgQueue.TryDequeue(out msg);
                if (msg == null)
                { break; }
                if (msg.msgType == Constants.RMF_MSG_CONNECT)
                {
                    // Not applicable for c# version (handled in onConnected)
                    throw new System.NotImplementedException();
                }
                else if (msg.msgType == Constants.RMF_MSG_FILEINFO)
                {
                    byte[] header = RemoteFileUtil.packHeader(Constants.RMF_CMD_START_ADDR, false);
                    if (transmitHandler != null)
                    {
                        transmitHandler.send(header, BitConverter.GetBytes(msg.msgData1));
                    }
                }
                else if (msg.msgType == Constants.RMF_MSG_WRITE_DATA)
                {
                    byte[] header = RemoteFileUtil.packHeader(msg.msgData1, false);
                    if (transmitHandler != null)
                    {
                        transmitHandler.send(header, msg.msgData3);
                    }
                }
                else if (msg.msgType == Constants.RMF_MSG_FILEOPEN)
                {
                    byte[] header = RemoteFileUtil.packHeader(Constants.RMF_CMD_START_ADDR, false);
                    List<byte> data = RemoteFileUtil.packFileOpen(msg.msgData1);
                    if (transmitHandler != null)
                    { transmitHandler.send(header, msg.msgData3); }
                }
                else
                {
                    throw new System.NotImplementedException("Unknown msgType");
                }
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
