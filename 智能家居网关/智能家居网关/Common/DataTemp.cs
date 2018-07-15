using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    public static class DataTemp
    {
        public static PortEnvResponse EnvData { get; set; }

        public static DateTime DateTime { get; set; }

        /// <summary>
        /// 存放CMN对应的SockID <Cmn, SockID>
        /// </summary>
        public static Dictionary<uint, uint> CmnSockID = new Dictionary<uint, uint>();

        /// <summary>
        /// 暂存HTTP应答
        /// </summary>
        public static HttpControlResponse httpControlResponse { get; set; }

        public static PortControlResponse portControlResponse { get; set; }
    }
}
