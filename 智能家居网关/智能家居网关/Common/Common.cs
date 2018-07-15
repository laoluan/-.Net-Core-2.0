using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace 智能家居网关
{
    public static class Common
    {
        static uint cmn = 0;
        /// <summary>
        /// 存放Token
        /// </summary>
        public static Dictionary<string, string> Tokens = new Dictionary<string, string>();

        /// <summary>
        /// (一期) 临时存放用户名密码, 后期会引用数据库
        /// </summary>
        public static Dictionary<string, string> User = new Dictionary<string, string>()
        {
            { "admin","admin" },
            { "guest","guest"}
        };



        /// <summary>
        /// 生成Token
        /// </summary>
        /// <returns></returns>
        public static string MD5Encrypt64()
        {
            //当前的年月日时分秒加时钟周期
            string str = DateTime.Now.ToString("yyyyMMddHHmmss") + Environment.TickCount.ToString();

            //实例化一个md5对像
            MD5 md5 = MD5.Create();

            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            return Convert.ToBase64String(s);
        }

        public static uint GetCmn()
        {
            if (cmn > 255)
            {
                cmn = 0;
            }
            else
            {
                cmn++;
            }

            return cmn;
        }

    }
}
