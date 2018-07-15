using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    /// <summary>
    /// 应答模型
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// 请求状态
        /// </summary>
        public ResultStatus Status { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        public string ResultMessage { get; set; }
    }    
}
