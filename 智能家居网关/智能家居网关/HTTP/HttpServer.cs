using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SerialPortLib;
using System.Configuration;
using System.Threading;

namespace 智能家居网关
{
    public class HttpServer
    {
        SerialPortInput serialPortInput = new SerialPortInput();

        public HttpServer()
        {
            #region 初始化串口
            int baudRate = 38400;
            string portName = "COM1";
            try
            {
                baudRate = Convert.ToInt32(ConfigurationManager.AppSettings["BaudRate"]);
            }
            catch (Exception)
            {
                Console.WriteLine("波特率获取失败");
                return;
            }

            if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            {
                portName = ConfigurationManager.AppSettings["WinPortName"];
            }
            else
            {
                portName = ConfigurationManager.AppSettings["LinuxPortName"];
            }

            if (string.IsNullOrWhiteSpace(portName))
            {
                Console.WriteLine("串口获取失败");
                return;
            }

            serialPortInput.ConnectionStatusChanged += SerialPortInput_ConnectionStatusChanged;
            serialPortInput.MessageReceived += SerialPortInput_MessageReceived;
            serialPortInput.SetPort(portName, baudRate);
            serialPortInput.Connect();

            Console.Write($"等待串口{portName}连接");
            while (!serialPortInput.IsConnected)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine($"\n串口{portName}连接成功");

            serialPortInput.SendMessage(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x0A, 0x00, 0x01, 0x00 });
            #endregion

            #region 初始化服务器
            var listener = new System.Net.Http.HttpListener(IPAddress.Any, 8081);
            try
            {
                listener.Request += async (sender, context) =>
                {
                    var request = context.Request;
                    var response = context.Response;

                    //如果是GET请求
                    if (request.HttpMethod == HttpMethods.Get)
                    {
                        string content = @"<h2>提供POST方法测试</h2>
                                <form method=""POST"" action=""http://192.168.123.166:8081/login"">
                                    <input name=""data""></input>                                   
                                    <button type=""submit"">Send</button>
                                </form>";

                        await response.WriteContentAsync(MakeDocument(content));
                    }

                    //如果是POST请求
                    else if (request.HttpMethod == HttpMethods.Post)
                    {
                        var data = await request.ReadUrlEncodedContentAsync();

                        //登录
                        if (request.Url.LocalPath.ToLower().Contains("login"))
                        {
                            ProcessLogin(data, response);
                        }

                        //获取数据
                        else if (request.Url.LocalPath.ToLower().Contains("getdata"))
                        {
                            ProcessGetData(data, response);
                        }

                        //控制开关
                        else if (request.Url.LocalPath.ToLower().Contains("control"))
                        {
                            ProcessControl(data, response);
                        }

                        //登出
                        else if (request.Url.LocalPath.ToLower().Contains("logout"))
                        {
                            ProcessLogOut(data, response);
                        }
                        else
                        {
                            //请求出错
                        }
                    }
                    else
                    {
                        response.MethodNotAllowed();
                    }
                    response.Close();
                };
                listener.Start();

                Console.WriteLine("服务器已启动...");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                listener.Close();

            }
            #endregion
        }


        /// <summary>
        /// Http用户登录
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="response"></param>
        private async void ProcessLogin(IDictionary<string, string> postData, System.Net.Http.HttpListenerResponse response)
        {
            ResponseModel responseModel = new ResponseModel();
            //POST数据中是否有'data'这个Key
            if (null == postData || !postData.ContainsKey("data"))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "Login Post No 'data' Key";
                Console.WriteLine("Login Post No 'data' Key");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //data Key 对应的 Value 是否有值
            if (string.IsNullOrWhiteSpace(postData["data"]))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "Login POST 'data' Key No Value";
                Console.WriteLine("Login POST 'data' Key No Value");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            User user;
            try
            {
                user = JsonConvert.DeserializeObject<User>(postData["data"]);
            }
            catch (Exception ex)
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = $"Login User Json Err: {ex.Message}";
                Console.WriteLine($"Login User Json Err: {ex.Message}");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //用户密码是否正确
            if (!(Common.User.ContainsKey(user.UID) && Common.User[user.UID] == user.PWD))
            {

                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "Login Username Or Password Error";
                Console.WriteLine("Login Username Or Password Error");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }


            //登录成功
            string token = Common.MD5Encrypt64();
            responseModel.Status = ResultStatus.Success;
            responseModel.ResultMessage = "Login Success";
            responseModel.Data = $"{{'token':'{token}'}}";
            Console.WriteLine("Login Success");

            //如果UID对应的token已存在, 更新token
            if (Common.Tokens.ContainsKey(user.UID))
            {
                Common.Tokens[user.UID] = token;
            }
            //否则根据UID新建token
            else
            {
                Common.Tokens.Add(user.UID, token);
            }

            await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
        }

        /// <summary>
        /// Http获取环境数据请求
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="response"></param>
        private async void ProcessGetData(IDictionary<string, string> postData, System.Net.Http.HttpListenerResponse response)
        {
            ResponseModel responseModel = new ResponseModel();
            //POST数据中是否有'data'这个Key
            if (null == postData || !postData.ContainsKey("data"))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "GetData No 'data' Key";
                Console.WriteLine("GetData No 'data' Key");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //data Key 对应的 Value 是否有值
            if (string.IsNullOrWhiteSpace(postData["data"]))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "GetData POST 'data' Key No Value";
                Console.WriteLine("GetData POST 'data' Key No Value");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            HttpEnvRequest httpEnvRequest;
            try
            {
                httpEnvRequest = JsonConvert.DeserializeObject<HttpEnvRequest>(postData["data"]);
            }
            catch (Exception ex)
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = $"GetData Json Err: {ex.Message}";
                Console.WriteLine($"GetData Json Err: {ex.Message}");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //token校验           
            if (!Common.Tokens.ContainsValue(httpEnvRequest.Token))
            {
                Console.WriteLine("token不存在");
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "token不存在";
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //校验成功, 返回数据
            HttpEnvResponse httpEnvResponse = HttpEnvResponse.Pack(DataTemp.EnvData);
            responseModel.Status = ResultStatus.Success;
            responseModel.ResultMessage = "环境数据";
            responseModel.Data = httpEnvResponse;
            await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));

            //为了提高性能, 每一次返回的都是上次获取的环境数据
            if (DateTime.Now - DataTemp.DateTime > new TimeSpan(0, 0, 4))
            {
                Console.WriteLine("DataTemp.DateTimes数据为5秒之前的, 向串口请求新的数据");

                PortEnvRequest portEnvRequest = new PortEnvRequest();
                portEnvRequest.cmn = Common.GetCmn();

                ProcessPortEnvRequest(portEnvRequest);
            }
            else
            {
                Console.WriteLine("DataTemp.DateTimes数据为5秒之内的, 无需向串口请求新的数据");
            }
        }


        /// <summary>
        /// Http继电器控制请求
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="response"></param>
        private async void ProcessControl(IDictionary<string, string> postData, System.Net.Http.HttpListenerResponse response)
        {
            ResponseModel responseModel = new ResponseModel();
            //POST数据中是否有'data'这个Key
            if (null == postData || !postData.ContainsKey("data"))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "GetData No 'data' Key";
                Console.WriteLine("GetData No 'data' Key");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //data Key 对应的 Value 是否有值
            if (string.IsNullOrWhiteSpace(postData["data"]))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "GetData POST 'data' Key No Value";
                Console.WriteLine("GetData POST 'data' Key No Value");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }
            ////
            HttpControlRequest httpControlRequest;
            try
            {
                httpControlRequest = JsonConvert.DeserializeObject<HttpControlRequest>(postData["data"]);
            }
            catch (Exception ex)
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = $"GetData Json Err: {ex.Message}";
                Console.WriteLine($"GetData Json Err: {ex.Message}");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //token校验           
            if (!Common.Tokens.ContainsValue(httpControlRequest.Token))
            {
                Console.WriteLine("token不存在");
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "token不存在";
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }



            PortControlRequest portControlRequest = new PortControlRequest();
            portControlRequest.cmn = Common.GetCmn();
            portControlRequest.sockId = httpControlRequest.SockID;
            portControlRequest.onOff = httpControlRequest.OnOff;

            DataTemp.CmnSockID.Add(portControlRequest.cmn, portControlRequest.sockId);

            ProcessPortControlRequest(portControlRequest, response);
        }

        /// <summary>
        /// 处理登出
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="response"></param>
        private async void ProcessLogOut(IDictionary<string, string> postData, System.Net.Http.HttpListenerResponse response)
        {
            ResponseModel responseModel = new ResponseModel();
            //POST数据中是否有'data'这个Key
            if (null == postData || !postData.ContainsKey("data"))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "LogOut No Post Data";
                Console.WriteLine("LogOut No Post Data");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //data Key 对应的 Value 是否有值
            if (string.IsNullOrWhiteSpace(postData["data"]))
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "LogOut POST Data is Null";
                Console.WriteLine("LogOut POST Data is Null");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            User user;
            try
            {
                user = JsonConvert.DeserializeObject<User>(postData["data"]);
            }
            catch (Exception ex)
            {
                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = $"LogOut User Json Err: {ex.Message}";
                Console.WriteLine($"LogOut User Json Err: {ex.Message}");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }

            //用户密码是否正确
            if (!(Common.User.ContainsKey(user.UID) && Common.User[user.UID] == user.PWD))
            {

                responseModel.Status = ResultStatus.Error;
                responseModel.ResultMessage = "LogOut UserName Or Password Error";
                Console.WriteLine("LogOut UserName Or Password Error");
                await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
                return;
            }


            //登出
            responseModel.Status = ResultStatus.Success;
            Console.WriteLine($"{user.UID} LogOut Success");

            if (Common.Tokens.ContainsKey(user.UID))
            {
                Common.Tokens.Remove(user.UID);
                responseModel.ResultMessage = $"{user.UID} 登出成功";
            }
            else
            {
                responseModel.ResultMessage = "用户已登出, 无需重复提交";
            }

            await response.WriteContentAsync(JsonConvert.SerializeObject(responseModel));
        }

        /// <summary>
        /// 串口环境数据请求
        /// </summary>
        /// <param name="portEnvRequest"></param>
        /// <returns></returns>
        private bool ProcessPortEnvRequest(PortEnvRequest portEnvRequest)
        {
            bool isSend = false;

            byte[] buffer = PortEnvRequest.Pack(portEnvRequest);
            isSend = serialPortInput.SendMessage(buffer);
            if (!isSend)
            {
                Console.WriteLine("串口数据发送失败");
                return false;
            }
            else
            {
                Console.WriteLine($"串口发送: + {BitConverter.ToString(buffer)}");
                return true;
            }
        }

        /// <summary>
        /// 串口控制继电器 数据发送
        /// </summary>
        /// <param name="portControlRequest"></param>
        /// <returns></returns>
        private async void ProcessPortControlRequest(PortControlRequest portControlRequest, System.Net.Http.HttpListenerResponse response)
        {
            bool isSend = false;

            byte[] buffer = PortControlRequest.Pack(portControlRequest);

            //清除缓存
            DataTemp.httpControlResponse = null;

            isSend = serialPortInput.SendMessage(buffer);
            if (!isSend)
            {
                Console.WriteLine("串口控制数据发送失败");
                return;
            }
            else
            {
                Console.WriteLine($"串口发送: + {BitConverter.ToString(buffer)}");

                int i = 0;
                for (i = 0; i < 3; i++)
                {
                    if (DataTemp.portControlResponse == null)
                    {
                        //串口没回应, 等待1秒
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        //收到控制数据
                        if (!DataTemp.CmnSockID.ContainsKey(DataTemp.portControlResponse.Cmn))
                        {
                            Console.WriteLine($"cmn{DataTemp.portControlResponse.Cmn}不存在, 丢弃数据");
                        }
                        else
                        {
                            if (DataTemp.portControlResponse.Success == 0)
                            {
                                //控制成功
                                Console.WriteLine("控制成功");
                                DataTemp.httpControlResponse = new HttpControlResponse() { Success = "1" };
                            }
                            else
                            {
                                //控制失败
                                Console.WriteLine("控制失败");
                                DataTemp.httpControlResponse = new HttpControlResponse() { Success = "0" };
                            }
                            await response.WriteContentAsync(JsonConvert.SerializeObject(DataTemp.httpControlResponse));
                        }
                        DataTemp.portControlResponse = null;
                        break;
                    }
                }
                if (i == 2)
                {
                    Console.WriteLine("串口没在规定时间回应控制数据");
                    return;
                }
            }
        }

        /// <summary>
        /// 串口状态改变回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void SerialPortInput_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("串口改变: " + args.Connected.ToString());
        }

        /// <summary>
        /// 串口数据接收回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void SerialPortInput_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine("收到: " + BitConverter.ToString(args.Data));

            //获取msgid用来分辨是环境应答还是控制应答
            uint msgid = BitConverter.ToUInt16(args.Data, 4);

            //环境数据应答
            if (msgid == 2)
            {
                Console.WriteLine("收到环境应答");
                DataTemp.EnvData = PortEnvResponse.UnPack(args.Data);
                DataTemp.DateTime = DateTime.Now;
            }
            //控制应答
            else if (msgid == 4)
            {
                Console.WriteLine("收到控制应答");
                DataTemp.portControlResponse = PortControlResponse.Unpack(args.Data);               
            }
            else
            {
                Console.WriteLine($"串口应答msgid为{msgid}, 无法解析");
            }

        }

        /// <summary>
        /// 生成HTML文档
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string MakeDocument(object content)
        {
            return @"<html>
                        <head>
                            <title>Test</title>
                        </head>
                        <body>" +
                            content +
                        @"</body>
                    </html>";
        }
    }
}
