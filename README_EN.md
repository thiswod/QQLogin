# QQ Quick Login Tool

This is a QQ quick login tool developed based on C#. It can list locally logged-in QQ accounts and support one-click quick login to specified QQ applications.

> 注：本项目提供中文版README文档，您可以查看 [README.md](README.md) 获取中文说明。

## Main Features

1. **QQ Account List Display** - Shows locally logged-in QQ accounts, including avatars and QQ numbers
2. **GIF Avatar Support** - Supports displaying animated GIF format QQ avatars
3. **Quick Login Function** - Double-click on a QQ account to quickly log in to the specified application
4. **g_tk Calculation Tool** - Provides the function to convert skey to g_tk value, convenient for developers
5. **Multi-application Support** - Supports logging in to applications such as QQ Space, QQ Groups, Security Center, Member Center, and Weiyun
6. **Cookie Extraction** - Cookie information can be extracted for use in other applications after successful login

## Technical Features

- Developed using C# Windows Forms
- Custom HTTP request class for network communication
- Support for GIF animated avatars
- ListView control to display user list with selection feedback
- Based on QQ official login protocol to implement quick login
- Complete error handling mechanism

## Usage Instructions

1. After running the program, the system will automatically list locally logged-in QQ accounts
2. Double-click the QQ account you want to log in with, and the system will automatically log in to the QQ Group application
3. After successful login, the Cookie information will be displayed in the text box below
4. Click the calculate g_tk button, enter the skey value, and the system will calculate and copy the g_tk value to the clipboard

## Development Notes

### Core Classes

- **QQQuickLogin** - Core class implementing QQ quick login
- **HttpRequestClass** - Custom HTTP request class, replacing the native WebClient
- **Form1** - Main interface, responsible for UI interaction and account display
- **QQLoginType** - Defines login parameters for different QQ applications

### Important Methods

- `QQLoginType.Get_G_tk(string skey)` - Calculates QQ's g_tk value for API call authentication
- `QQQuickLogin.GetUins()` - Gets the list of locally logged-in QQ accounts
- `QQQuickLogin.Login(uins)` - Executes the quick login operation
- `QQQuickLogin.GetFace(uins)` - Gets QQ avatar information

### Project Structure

```
QQLogin/
├── EasyHTTP.cs          # HTTP request implementation
├── EasyJson.cs          # JSON parsing tool
├── Form1.cs             # Main interface code
├── Form1.Designer.cs    # Main interface design
├── InputDialog.cs       # Input dialog box
├── Program.cs           # Program entry point
├── QQQuickLogin.cs      # QQ quick login core class
├── common.cs            # Common utility methods
└── README.md            # Project documentation
```

## Notes

1. Please ensure that you have logged in to the QQ client locally before using this tool
2. Login operations require local QQ security module support
3. This tool is only for learning and development testing, please do not use it for illegal purposes
4. If you encounter security or functionality issues, please update your QQ client to the latest version in time

## License

This project is for learning and research purposes only.

## Development Environment

- Visual Studio 2022+
- .NET 8
- Windows 10/11 operating system