using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteFile
{



    public class Msg
    {
        public UInt32 msgType;
        public UInt32 msgData1;
        public UInt32 msgData2;
        public List<byte> msgData3;

        public Msg(uint _msgType, uint _msgData1, uint _msgData2, List<byte> _msgData3)
        {
            msgType  = _msgType;
            msgData1 = _msgData1;
            msgData2 = _msgData2;
            msgData3 = _msgData3;
        }
        
    }
}
