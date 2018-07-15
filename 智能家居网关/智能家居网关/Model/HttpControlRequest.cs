using System;
using System.Collections.Generic;
using System.Text;

namespace 智能家居网关
{
    public class HttpControlRequest
    {
        public int RoomID { get; set; }

        public string Token { get; set; }

        public byte SockID { get; set; } 

        public byte OnOff { get; set; }
    }
}
