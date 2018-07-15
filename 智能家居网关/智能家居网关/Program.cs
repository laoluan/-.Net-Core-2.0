using SerialPortLib;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace 智能家居网关
{
    class Program
    {
        static int baudRate = 38400;
        static string portName = "COM1";
        static SerialPortInput serialPortInput = new SerialPortInput();

        static void Main(string[] args)
        {
            //try
            //{
            //    baudRate = Convert.ToInt32(ConfigurationManager.AppSettings["BaudRate"]);
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("波特率获取失败");
            //    return;
            //}

            //if (Environment.OSVersion.Platform.ToString().StartsWith("Win"))
            //{
            //    portName = ConfigurationManager.AppSettings["WinPortName"];
            //}
            //else
            //{
            //    portName = ConfigurationManager.AppSettings["LinuxPortName"];
            //}

            //if (string.IsNullOrWhiteSpace(portName))
            //{
            //    Console.WriteLine("串口获取失败");
            //    return;
            //}

            //serialPortInput.ConnectionStatusChanged += SerialPortInput_ConnectionStatusChanged;
            //serialPortInput.MessageReceived += SerialPortInput_MessageReceived;
            //serialPortInput.SetPort(portName, baudRate);
            //serialPortInput.Connect();

            //Console.Write($"等待串口{portName}连接");
            //while (!serialPortInput.IsConnected)
            //{
            //    Console.Write(".");
            //    Thread.Sleep(1000);
            //}
            //Console.WriteLine($"\n串口{portName}连接成功");

            //serialPortInput.SendMessage(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x0A, 0x00, 0x01, 0x00 });
            //PortEnvRequest portEnvRequest = new PortEnvRequest();
            //portEnvRequest.cmn = 1;
            //RequestPort(portEnvRequest);
            Console.WriteLine("正在初始化, 请稍候...");
            HttpServer httpServer = new HttpServer();

            Console.ReadKey();
        }

        private static bool RequestPort(PortEnvRequest portEnvRequest)
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

        private static void SerialPortInput_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("串口改变: " + args.Connected.ToString());
        }

        private static void SerialPortInput_MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            Console.WriteLine("收到: " + BitConverter.ToString(args.Data));
        }
    }
}
