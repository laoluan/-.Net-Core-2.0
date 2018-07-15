using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    public class PortControlResponse
    {
        public uint Cmn { get; set; }
        public ushort MsgId { get; set; }

        public ushort MsgLen { get; set; }

        public ushort Success { get; set; }

        internal static PortControlResponse Unpack(byte[] receiveData)
        {
            PortControlResponse portControlResponse = new PortControlResponse();
            portControlResponse.Cmn = BitConverter.ToUInt32(receiveData, 0);
            portControlResponse.MsgId = BitConverter.ToUInt16(receiveData, 4);
            portControlResponse.MsgLen = BitConverter.ToUInt16(receiveData, 6);
            portControlResponse.Success = BitConverter.ToUInt16(receiveData, 8);            
            Console.WriteLine("调试输出");
            return portControlResponse;
        }
    }
}
