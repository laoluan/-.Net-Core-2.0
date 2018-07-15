# SmartHome
基于.Net Core 2.0的智能家居网关
---

#### 请使用带NuGet包管理的VS版本打开(VS2015及以上)

#### 引用框架:
> HttpListener for .NET Core [https://github.com/StefH/NETStandard.HttpListener]

>serialport-lib-dotnet [https://github.com/genielabs/serialport-lib-dotnet]

`app.config`存放默认端口及波特率

#### 项目介绍
将ZigBee协调器收集上发的环境数据解析后放到缓冲区，供客户端查询

将客户端的控制请求转发给ZigBee，实现开灯等开关继电器指令
