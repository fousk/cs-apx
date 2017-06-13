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
        public Apx.FileMap localFileMap, remoteFileMap;
        public List<File> requestedFiles = new List<File>();
        public string byteOrder = "<"; // use '<' for little endian, '>' for big endian
        public Guid InstanceID { get; private set; }    // Check that we use the right instance

        public TransmitHandler transmitHandler;

        EventWaitHandle waitHandle = new AutoResetEvent(false);
        ConcurrentQueue<Msg> msgQueue = new ConcurrentQueue<Msg>();

        public FileManager(Apx.FileMap setLocalFileMap, Apx.FileMap setRemoteFileMap)
        {
            this.InstanceID = Guid.NewGuid();
            localFileMap = setLocalFileMap;
            remoteFileMap = setRemoteFileMap;
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
            if (localFileMap._items.Count == 0)
            {
                throw new ArgumentException("No files in localFileMap, initialization not done");
            }
            foreach (File file in  localFileMap._items)
            {
                List<byte> fileData = RemoteFileUtil.packFileInfo(file, "<");
                throw new NotImplementedException("Fix Msg initialization (0, 0)");
                Msg msg = new Msg(Constants.RMF_MSG_FILEINFO, (uint)0, (uint)0, fileData);
                msgQueue.Enqueue(msg);
            }
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

        public void _processCmd(List<byte> data)
        {   
            if (data.Count >= 4)
            {
                uint cmd = BitConverter.ToUInt32(data.GetRange(0, 4).ToArray(), 0);
                if (cmd == Constants.RMF_CMD_FILE_INFO)
                {
                    throw new NotImplementedException();
                }
                else if (cmd == Constants.RMF_CMD_FILE_OPEN)
                {
                    uint address = RemoteFileUtil.unPackFileOpen(data, "<");
                    Apx.File file = localFileMap.findByAddress(address);
                    if (file.address == uint.MaxValue)
                    {
                        file.open();
                        List<byte> fileContent = file.read(0, (int)file.length);
                        if (fileContent.Count > 0)
                        {
                            Msg msg = new Msg(Constants.RMF_CMD_FILE_CLOSE, file.address, 0, fileContent);
                            msg.msgData1 = Constants.RMF_CMD_FILE_CLOSE;
                            msg.msgData2 = file.address;
                            msg.msgData3 = fileContent;

                            msgQueue.Enqueue(msg);
                        }
                    }

                    throw new NotImplementedException();
                }
                else if (cmd == Constants.RMF_CMD_FILE_CLOSE)
                {
                    throw new NotImplementedException();
                }
                else
                { throw new ArgumentException("Unknown command, cannot process"); }


            }
            else
            {
                throw new ArgumentException("too short command to proccess");
            }



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
