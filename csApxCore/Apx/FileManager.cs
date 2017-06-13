using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using RemoteFile;

namespace Apx
{
    class FileManager : RemoteFile.FileManager
    {
        protected static FileMap localFileMap = new FileMap();
        protected static FileMap remoteFileMap = new FileMap();
        protected static List<Apx.File> requestedFiles = new List<Apx.File>();
        
        public FileManager() : base(localFileMap, localFileMap)
        {

        }
        
        public void attachNodeData(Apx.NodeData nodeData)
        {
            if (nodeData.inPortDataFile != null)
            {
                attachLocalFile(nodeData.inPortDataFile);
                Console.WriteLine(nodeData.inPortDataFile.name + " address: " + nodeData.inPortDataFile.address);
            }
            if (nodeData.outPortDataFile != null)
            {
                requestedFiles.Add(nodeData.outPortDataFile);
            }
            if (nodeData.definitionFile != null)
            {
                attachLocalFile(nodeData.definitionFile);
                Console.WriteLine(nodeData.definitionFile.name + " address: " + nodeData.definitionFile.address);
            }
        }

        public void attachLocalFile(Apx.File file)
        {
            localFileMap.insert(file);
        }

        public void attachRemoteFile(Apx.File file)
        {
            remoteFileMap.insert(file);
        }

        public override void onConnected(TransmitHandler handler)
        {
            transmitHandler = handler;
            if (localFileMap._items.Count == 0)
            {
                throw new ArgumentException("No files in localFileMap, initialization not done");
            }
            foreach (File file in localFileMap._items)
            {
                Console.WriteLine(file.name);

                Msg msg = new Msg(RemoteFile.Constants.RMF_MSG_FILEINFO, (uint)0, (uint)0, (object)file);
                msgQueue.Add(msg);
                //throw new NotImplementedException("Fix Msg initialization (0, 0)");
                
                
            }
        }

        protected override void processMessage(Msg msg)
        {
            if (msg.msgType == RemoteFile.Constants.RMF_MSG_CONNECT)
            {
                throw new System.ArgumentException("Not applicable for c# version (handled in onConnected)");
            }
            else if (msg.msgType == RemoteFile.Constants.RMF_MSG_FILEINFO)
            {
                Apx.File file = (Apx.File)msg.msgData3;
                Console.WriteLine("Name of file: " + file.name);
                
                List<byte> data = RemoteFileUtil.packHeader(RemoteFile.Constants.RMF_CMD_START_ADDR, false);
                List<byte> payload = RemoteFileUtil.packFileInfo(file, "<");
                data.AddRange(payload);

                if (transmitHandler != null)
                {
                    transmitHandler.send(data);
                }
                
            }
            /*
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
             */
            else
            {
                throw new System.ArgumentException("Unknown msgType");
            }
        }

        protected override void _processCmd(List<byte> data)
        {
            if (data.Count >= 4)
            {
                uint cmd = BitConverter.ToUInt32(data.GetRange(0, 4).ToArray(), 0);
                if (cmd == RemoteFile.Constants.RMF_CMD_FILE_INFO)
                {
                    throw new NotImplementedException();
                }
                else if (cmd == RemoteFile.Constants.RMF_CMD_FILE_OPEN)
                {
                    uint address = RemoteFileUtil.unPackFileOpen(data, "<");
                    Apx.File file = localFileMap.findByAddress(address);
                    if (file.address == uint.MaxValue)
                    {
                        file.open();
                        List<byte> fileContent = file.read(0, (int)file.length);
                        if (fileContent.Count > 0)
                        {
                            Msg msg = new Msg(RemoteFile.Constants.RMF_CMD_FILE_CLOSE, file.address, 0, fileContent);
                            msg.msgData1 = RemoteFile.Constants.RMF_CMD_FILE_CLOSE;
                            msg.msgData2 = file.address;
                            msg.msgData3 = fileContent;

                            msgQueue.Add(msg);
                        }
                    }

                    throw new NotImplementedException();
                }
                else if (cmd == RemoteFile.Constants.RMF_CMD_FILE_CLOSE)
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

        protected override void _processFileWrite(uint address, bool more_bit, List<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
