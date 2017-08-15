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

        public virtual void stop()
        {

        }
    }

    public abstract class FileManager : ReceiveHandler
    {
        protected string byteOrder = "<"; // use '<' for little endian, '>' for big endian
        protected Guid InstanceID { get; private set; }    // Check that we use the right instance
        protected BlockingCollection<Msg> msgQueue = new BlockingCollection<Msg>();
        protected TransmitHandler transmitHandler;
        protected Thread workerThread;
        protected bool isWorkerThreadActive = false;

        protected EventWaitHandle waitHandle;
        
        public FileManager(Apx.FileMap setLocalFileMap, Apx.FileMap setRemoteFileMap)
        {
            this.InstanceID = Guid.NewGuid();
            waitHandle = new AutoResetEvent(false);
        }

        
        public void start()
        {
            workerThread = new Thread(new ThreadStart(worker));
            workerThread.Name = "Filemanager workerThread";
            workerThread.IsBackground = true;
            workerThread.Start();
            isWorkerThreadActive = true;
        }


        public void stop()
        {
            if (isWorkerThreadActive)
            {
                Console.WriteLine("stopping Filemanager workerThread");
                msgQueue.Add(null);
                workerThread.Join();
                isWorkerThreadActive = false;
            }
            
        }


        public void worker()
        {
            Msg msg;
            while (true)
            {
                msg = msgQueue.Take();

                if (msg == null)
                { break; }

                processMessage(msg);
                
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
                    _processFileWrite(header.address, header.more_bit, msg.GetRange(header.bytes_parsed, msg.Count - header.bytes_parsed));                
                }
                else
                {
                    throw new ArgumentException("invalid address: " + header.address.ToString());
                }
            }
            // else do nothing
        }

        
        protected abstract void _processCmd(List<byte> data);
        protected abstract void _processFileWrite(uint address, bool more_bit, List<byte> data);
        protected abstract void processMessage(Msg msg);


        public void Enqueue(Msg msg)
        {
            msgQueue.Add(msg);
            waitHandle.Set();
        }



    }

}
