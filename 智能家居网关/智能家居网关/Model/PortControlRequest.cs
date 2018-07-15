using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    public class PortControlRequest
    {
        public uint cmn;

        private readonly ushort msgId = 3;

        private readonly ushort msgLen = 10;

        public byte sockId;

        public byte onOff;

        internal static byte[] Pack(PortControlRequest portControlRequest)
        {
            List<byte> sendBuffer = new List<byte>();
            sendBuffer.AddRange(BitConverter.GetBytes(portControlRequest.cmn));
            sendBuffer.AddRange(BitConverter.GetBytes(portControlRequest.msgId));
            sendBuffer.AddRange(BitConverter.GetBytes(portControlRequest.msgLen));
            sendBuffer.AddRange(BitConverter.GetBytes(portControlRequest.sockId));
            sendBuffer.AddRange(BitConverter.GetBytes(portControlRequest.onOff));

            return sendBuffer.ToArray();
        }
    }
}
