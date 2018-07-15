using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    /// <summary>
    /// 回应数据
    /// </summary>
    public class HttpEnvResponse
    {
        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomID { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public string temp { get; set; }

        /// <summary>
        /// 光照
        /// </summary>
        public string light { get; set; }

        /// <summary>
        /// 湿度
        /// </summary>
        public string humi { get; set; }

        /// <summary>
        /// 5S通过的人数
        /// </summary>
        public string human { get; set; }

        /// <summary>
        /// 一氧化碳
        /// </summary>
        public string co { get; set; }

        /// <summary>
        /// PM2.5
        /// </summary>
        public string pm25 { get; set; }

        internal static HttpEnvResponse Pack(PortEnvResponse portEnvResponse)
        {
            HttpEnvResponse httpEnvResponse = new HttpEnvResponse();
            httpEnvResponse.RoomID = "1";
            httpEnvResponse.temp = portEnvResponse.temp.ToString();
            httpEnvResponse.light = portEnvResponse.light.ToString();
            httpEnvResponse.humi = portEnvResponse.humi.ToString();
            httpEnvResponse.human = portEnvResponse.human.ToString();
            httpEnvResponse.co = portEnvResponse.co.ToString();
            httpEnvResponse.pm25 = portEnvResponse.pm25.ToString();

            return httpEnvResponse;
        }
    }
}
