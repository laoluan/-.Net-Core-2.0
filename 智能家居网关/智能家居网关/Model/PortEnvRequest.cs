using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    /// <summary>
    /// 向串口请求环境数据
    /// </summary>
    public class PortEnvRequest
    {
        public uint cmn;

        private readonly ushort msgId = 1;

        private readonly ushort msgLen = 10;

        private readonly ushort reqId = 1;

        internal static byte[] Pack(PortEnvRequest portEnvRequest)
        {
            List<byte> sendBuffer = new List<byte>();
            sendBuffer.AddRange(BitConverter.GetBytes(portEnvRequest.cmn));
            sendBuffer.AddRange(BitConverter.GetBytes(portEnvRequest.msgId));
            sendBuffer.AddRange(BitConverter.GetBytes(portEnvRequest.msgLen));
            sendBuffer.AddRange(BitConverter.GetBytes(portEnvRequest.reqId));

            return sendBuffer.ToArray();
        }


    }
}
