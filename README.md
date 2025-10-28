# QQ快速登录工具

这是一个基于C#开发的QQ快速登录工具，可以列出本地已登录的QQ账号，并支持一键快速登录到指定的QQ应用。

## 主要功能

1. **QQ账号列表显示** - 展示本地已登录的QQ账号，包括头像和QQ号
2. **GIF头像支持** - 支持显示动画GIF格式的QQ头像
3. **快速登录功能** - 双击QQ账号可快速登录到指定应用
4. **g_tk计算工具** - 提供skey转g_tk值的功能，方便开发者使用
5. **多应用支持** - 支持登录到QQ空间、QQ群、安全中心、会员中心和微云等应用
6. **Cookie提取** - 登录成功后可提取Cookie信息用于其他应用

## 技术特点

- 使用C# Windows Forms开发
- 自定义HTTP请求类实现网络通信
- 支持GIF动画头像显示
- ListView控件展示用户列表，支持选中反馈
- 基于QQ官方登录协议实现快速登录
- 提供完整的错误处理机制

## 使用方法

1. 运行程序后，系统会自动列出本地已登录的QQ账号
2. 双击想要登录的QQ账号，系统将自动登录到QQ群应用
3. 登录成功后，Cookie信息将显示在下方文本框中
4. 点击计算g_tk按钮，输入skey值，系统将计算并复制g_tk值到剪贴板

## 开发说明

### 核心类

- **QQQuickLogin** - 实现QQ快速登录的核心类
- **HttpRequestClass** - 自定义HTTP请求类，替代原生WebClient
- **Form1** - 主界面，负责UI交互和账号显示
- **QQLoginType** - 定义不同QQ应用的登录参数

### 重要方法

- `QQLoginType.Get_G_tk(string skey)` - 计算QQ的g_tk值，用于API调用认证
- `QQQuickLogin.GetUins()` - 获取本地已登录的QQ账号列表
- `QQQuickLogin.Login(uins)` - 执行快速登录操作
- `QQQuickLogin.GetFace(uins)` - 获取QQ头像信息

### 项目结构

```
QQLogin/
├── EasyHTTP.cs          # HTTP请求相关实现
├── EasyJson.cs          # JSON解析工具
├── Form1.cs             # 主界面代码
├── Form1.Designer.cs    # 主界面设计
├── InputDialog.cs       # 输入对话框
├── Program.cs           # 程序入口
├── QQQuickLogin.cs      # QQ快速登录核心类
├── common.cs            # 通用工具方法
└── README.md            # 项目说明文档
```

## 注意事项

1. 使用该工具前请确保已在本地登录QQ客户端
2. 登录操作需要本地QQ安全模块支持
3. 本工具仅用于学习和开发测试，请勿用于非法用途
4. 如有安全或功能问题，请及时更新QQ客户端至最新版本

## 许可证

本项目仅供学习研究使用。

## 开发环境

- Visual Studio 2022+
- .NET 8
- Windows 10/11 操作系统