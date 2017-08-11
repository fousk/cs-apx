using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using RemoteFile;

namespace Apx
{
    public class FileManager : RemoteFile.FileManager
    {
        protected static FileMap localFileMap;
        protected static FileMap remoteFileMap;
        protected static List<Apx.File> requestedFiles;
        

        public FileManager() : base(localFileMap, localFileMap)
        {
            localFileMap = new FileMap();
            remoteFileMap = new FileMap();
            requestedFiles = new List<Apx.File>();
        }
        

        public void attachNodeData(Apx.NodeData nodeData)
        {
            if (nodeData.inPortDataFile != null)
            {
                requestedFiles.Add(nodeData.inPortDataFile);
            }
            if (nodeData.outPortDataFile != null)
            {
                attachLocalFile(nodeData.outPortDataFile);
                Console.WriteLine(nodeData.outPortDataFile.name + " address: " + nodeData.outPortDataFile.address); 
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
            if (localFileMap.fileList.Count == 0)
            {
                throw new ArgumentException("No files in localFileMap, initialization not done");
            }
            foreach (File file in localFileMap.fileList)
            {
                Console.WriteLine(file.name);

                Msg msg = new Msg(RemoteFile.Constants.RMF_MSG_FILEINFO, (uint)0, (uint)0, (object)file);
                msgQueue.Add(msg);
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
                Console.WriteLine("processMessage, Name of file: " + file.name);
                
                List<byte> data = RemoteFileUtil.packHeader(RemoteFile.Constants.RMF_CMD_START_ADDR, false);
                List<byte> payload = RemoteFileUtil.packFileInfo(file, "<");
                data.AddRange(payload);

                if (transmitHandler != null)
                { transmitHandler.send(data); }
            }
            else if (msg.msgType == RemoteFile.Constants.RMF_MSG_WRITE_DATA)
            {
                uint address = msg.msgData1;
                List<byte> data = RemoteFileUtil.packHeader(address); 
                List<byte> payload = (List<byte>)msg.msgData3;
                data.AddRange(payload);

                if (transmitHandler != null)
                { transmitHandler.send(data); }
            }
            else if (msg.msgType == RemoteFile.Constants.RMF_MSG_FILEOPEN)
            {
                List<byte> data = RemoteFileUtil.packHeader(RemoteFile.Constants.RMF_CMD_START_ADDR);
                List<byte> payload = RemoteFileUtil.packFileOpen(msg.msgData1);
                data.AddRange(payload);

                if (transmitHandler != null)
                { transmitHandler.send(data); }
            }
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
                    RemoteFile.File remoteFile = RemoteFileUtil.unPackFileInfo(data);
                    for (int i = 0; i < requestedFiles.Count; i++)
                    {
                        if (requestedFiles[i].name == remoteFile.name)
                        {
                            requestedFiles[i].address = remoteFile.address;
                            requestedFiles[i].fileType= remoteFile.fileType;
                            requestedFiles[i].digestType= remoteFile.digestType;
                            requestedFiles[i].digestData= remoteFile.digestData;
                            requestedFiles[i].open();
                            remoteFileMap.insert(requestedFiles[i]);
                            Msg msg = new Msg(RemoteFile.Constants.RMF_MSG_FILEOPEN, 0, 0, requestedFiles[i].address);
                            requestedFiles.RemoveAt(i);
                            msgQueue.Add(msg);
                        }
                        else
                        {
                            remoteFileMap.insert(remoteFile);
                        }
                    }

                }
                else if (cmd == RemoteFile.Constants.RMF_CMD_FILE_OPEN)
                {
                    uint address = RemoteFileUtil.unPackFileOpen(data, "<");
                    Console.WriteLine("received open command, address: " + address);
                    Apx.File file = localFileMap.findByAddress(address);
                    if (file != null)
                    {
                        Console.WriteLine("received open command, name: " + file.name);
                        file.open();
                        List<byte> fileContent = file.read(0, (int)file.length);
                        if (fileContent.Count > 0)
                        {
                            Msg msg = new Msg(RemoteFile.Constants.RMF_MSG_WRITE_DATA, file.address, 0, fileContent);
                            msgQueue.Add(msg);
                        }
                    }

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
        }


        protected override void _processFileWrite(uint address, bool more_bit, List<byte> data)
        {
            File remoteFile = remoteFileMap.findByAddress(address);
            if ((remoteFile != null) && (remoteFile.isOpen == true))
            {
                int offset = (int)address - (int)remoteFile.address;
                if ((offset >= 0) && (offset+data.Count <= remoteFile.length))
                {
                    remoteFile.write((uint)offset, data, more_bit);
                }
                else
                    throw new ArgumentException("_processFileWrite couldn't write (outside of memory)");
            }
            else
            {
                Console.WriteLine("_processFileWrite: Nothing written");
            }
        }
    }
}
