using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    /// <summary>
    /// 串口应答的环境数据
    /// </summary>
    public class PortEnvResponse
    {
        /// <summary>
        /// 请求标志
        /// </summary>
        public uint cmn { get; set; }

        /// <summary>
        /// 消息号
        /// </summary>
        public ushort msgId { get; set; }

        /// <summary>
        /// 消息长度
        /// </summary>
        public ushort msgLen { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public ushort reqId { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public ushort temp { get; set; }

        /// <summary>
        /// 光照
        /// </summary>
        public ushort light { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public ushort humi { get; set; }

        /// <summary>
        /// 5S通过的人数
        /// </summary>
        public ushort human { get; set; }

        /// <summary>
        /// 一氧化碳
        /// </summary>
        public ushort co { get; set; }

        /// <summary>
        /// PM2.5
        /// </summary>
        public ushort pm25 { get; set; }

        internal static PortEnvResponse UnPack(byte[] receiveData)
        {
            PortEnvResponse portEnvResponse = new PortEnvResponse();
            portEnvResponse.cmn = BitConverter.ToUInt32(receiveData, 0);
            portEnvResponse.msgId = BitConverter.ToUInt16(receiveData, 4);
            portEnvResponse.msgLen = BitConverter.ToUInt16(receiveData, 6);
            portEnvResponse.temp = BitConverter.ToUInt16(receiveData, 8);
            portEnvResponse.light = BitConverter.ToUInt16(receiveData, 10);
            portEnvResponse.humi = BitConverter.ToUInt16(receiveData, 12);
            portEnvResponse.human = Convert.ToUInt16( BitConverter.ToChar(receiveData, 14));
            portEnvResponse.co = BitConverter.ToUInt16(receiveData, 15);
            portEnvResponse.pm25 = BitConverter.ToUInt16(receiveData, 17);

            return portEnvResponse;
        }
    }
}
