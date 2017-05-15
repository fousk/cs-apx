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

        public virtual void onMsgReceived(List<byte> msg)
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
                    throw new System.ArgumentException("Not applicable for c# version (handled in onConnected)");
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
                    throw new System.ArgumentException("Unknown msgType");
                }
            }
        }

        public override void onConnected(TransmitHandler handler)
        {
            transmitHandler = handler;
            /*foreach (File file in  localFileMap)
            {
                continue here...
            }*/
        }

        public override void onMsgReceived(List<byte> msg)
        {
            RemoteFileUtil.headerReturn header = RemoteFileUtil.unpackHeader(msg.ToArray());
            if (header.bytes_parsed > 0)
            {
                if (header.address == Constants.RMF_CMD_START_ADDR)
                {
                    _processCmd(msg.GetRange(header.bytes_parsed, msg.Count - header.bytes_parsed));
                }
                else if (header.address < Constants.RMF_CMD_START_ADDR)
                {
                    _processFileWrite(header.address, header.more_bit, msg.GetRange(header.bytes_parsed, msg.Count - header.bytes_parsed));                }
                else
                {
                    throw new ArgumentException("invalid address: " + header.address.ToString());
                }
            }
            // else do nothing
        }

        public void _processCmd(List<byte> cmd)
        {   
            throw new NotImplementedException();
        }

        public void _processFileWrite(uint address, bool more_bit, List<byte> data)
        {
            throw new NotImplementedException();
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
