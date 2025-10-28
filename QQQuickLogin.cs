using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QQLogin
{
    public enum QQAppType
    {
        Qzone,//空间
        Qun,//群
        Accounts,//安全中心
        Vip,//会员中心
        Weiyun,//微云
    }
    public static class QQLoginType
    {

        ///APPID=pt_aid=

        /// <summary>
        /// 获取QQ登录类型
        /// </summary>
        /// <param name="appType"></param>
        /// <returns>(S_URL,APPID,DAID)</returns>
        public static (string,int,int)? Get(QQAppType appType)
        {
            switch (appType)
            {
                case QQAppType.Qzone:
                    return ("https://qzs.qq.com/qzone/v5/loginsucc.html?para=izone", 549000912,5);
                case QQAppType.Qun:
                    return ("https://qun.qq.com/", 715030901, 73);
                case QQAppType.Accounts:
                    return ("https://accounts.qq.com/homepage#/", 1600001573,0);//DAID未获取
                case QQAppType.Vip:
                    return ("https://vip.qq.com/loginsuccess.html", 8000201,0);//DAID未获取
                case QQAppType.Weiyun:
                    return ("https://www.weiyun.com/web/callback/common_qq_login_ok.html?login_succ", 527020901,0);//DAID未获取
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// QQ快速登录
    /// </summary>
    public class QQQuickLogin
    {
        HttpRequestClass _http = new HttpRequestClass();
        readonly int _AppId;
        readonly string _s_url;
        readonly int _Daid;
        string pt_local_token = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="App">应用类型</param>
        /// <exception cref="Exception"></exception>
        public QQQuickLogin(QQAppType App)
        {

            var result = QQLoginType.Get(appType: App);
            if (result.HasValue)
            {
                _s_url = result.Value.Item1;
                _AppId = result.Value.Item2;
                _Daid = result.Value.Item3;
            }
            else
            {
                throw new Exception("无法获取指定的应用配置");
            }
            //Edge浏览器UA
            _http.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            Dictionary<string, string> post_data = new Dictionary<string, string>() {
                {"proxy_url",""},// 登录框代理地址，留空表示使用默认
                {"daid",_Daid.ToString()},// 登录框显示模式，5表示新样式
                {"hide_title_bar","1"},// 是否隐藏登录框标题栏，0表示不隐藏，1表示隐藏
                {"low_login","0"},// 是否低版本登录，0表示不低版本登录，1表示低版本登录
                {"qlogin_auto_login","1"},// 是否自动登录，0表示不自动登录，1表示自动登录
                {"no_verifyimg","1"},// 是否禁用验证码登录，0表示不禁用，1表示禁用
                {"link_target","blank"},// 登录成功后的跳转方式，blank表示在新窗口跳转
                {"appid",_AppId.ToString()},// 应用ID，用于标识登录的应用
                {"style","22"},// 登录框样式，22表示新样式
                {"target","self"},// 登录成功后的跳转方式，self表示在当前窗口跳转，blank表示在新窗口跳转
                {"s_url",_s_url},// 登录成功后的跳转地址，留空表示使用默认
                {"pt_qr_app",""},// 二维码登录应用名称，留空表示使用默认
                {"pt_qr_link",""},// 二维码登录链接地址，留空表示使用默认
                {"self_regurl",""},// 新用户注册链接地址，留空表示使用默认
                {"pt_qr_help_link",""},// 二维码登录帮助链接地址，留空表示使用默认
                {"pt_no_auth","1"},// 是否禁用无密码登录，0表示不禁用，1表示禁用
            };
            _http.Open($"https://xui.ptlogin2.qq.com/cgi-bin/xlogin?{common.DictionaryToQueryString(post_data)}", HttpMethod.Get);
            _http.Send();
            pt_local_token = _http.CookieManager().GetCookieValue("pt_local_token");
        }
        public string Login(uins uins)
        {
            Dictionary<string,string> post_data = new Dictionary<string, string>()
            {
                {"clientuin",uins.uin.ToString()},
                {"r",common.refreshCode()},
                {"pt_local_tk",pt_local_token},
                {"callback","__jp1"},
            };
            _http.Open($"https://localhost.ptlogin2.qq.com:4301/pt_get_st?{common.DictionaryToQueryString(post_data)}",HttpMethod.Get);//登入
            _http.SetTemporaryHeader("Referer", "https://xui.ptlogin2.qq.com/");
            _http.Send();
            try
            {
                post_data = new Dictionary<string, string>();
                Regex regex = new Regex(@"\{uin:\s([\d]+),\skeyindex:\s([\d]+)\}");
                //取出两个结果
                Match match = regex.Match(_http.GetResponse().Body);
                if (match.Success)
                {
                    string uin = match.Groups[1].Value;
                    string keyindex = match.Groups[2].Value;
                    post_data.Add("uin", uin);
                    post_data.Add("keyindex", keyindex);
                }
                else
                {
                    throw new Exception("获取用户信息失败");
                }
                post_data.Add("pt_aid", _AppId.ToString());
                post_data.Add("daid", _Daid.ToString());
                post_data.Add("u1", _s_url);
                post_data.Add("pt_local_tk", pt_local_token);
                post_data.Add("pt_3rd_aid", "0");
                post_data.Add("ptopt", "1");
                post_data.Add("style", "40");
                _http.Open($"https://ssl.ptlogin2.qq.com/jump?{common.DictionaryToQueryString(post_data)}", HttpMethod.Get);
                _http.Send();//授权
                //(https://[\s\S]+?)',
                regex = new Regex(@"(https://[\s\S]+?)',");
                match = regex.Match(_http.GetResponse().Body);
                if (match.Success)
                {
                    string callback = match.Groups[1].Value;
                    _http.SetProxy("127.0.0.1:8888");
                    _http.Open(callback, HttpMethod.Get);
                    _http.SetTemporaryHeader("Referer", "https://xui.ptlogin2.qq.com/");
                    _http.SetTemporaryHeader("Connection", "keep-alive");
                    _http.Send();//跳转到应用
                    _http.RemoveProxy();
                }
                CookieManager cookieManager = _http.CookieManager();
                string p_skey = cookieManager.GetCookieValue("p_skey");
                string p_uin = cookieManager.GetCookieValue("p_uin");
                string skey = cookieManager.GetCookieValue("skey");
                cookieManager.ClearCookies();
                cookieManager.SetCookie("p_skey", p_skey);
                cookieManager.SetCookie("p_uin", p_uin);
                cookieManager.SetCookie("skey", skey);
                return _http.CookieManager().GetRawCookieString();
            }
            catch (Exception ex)
            {
                throw new Exception("获取用户信息失败：" + ex.Message);
            }
            
        }
        public List<uins>? GetUins()
        {
            List<uins> uins = new List<uins>();
            _http.Open($"https://localhost.ptlogin2.qq.com:4301/pt_get_uins?callback=ptui_getuins_CB&r={common.refreshCode()}&pt_local_tk={pt_local_token}", HttpMethod.Get);
            _http.SetTemporaryHeader("Referer", "https://xui.ptlogin2.qq.com/");
            _http.Send();
            Regex regex = new Regex(@"var\svar_sso_uin_list=([\s\S]+?);ptui_getuins_CB\(var_sso_uin_list\);");
            Match match = regex.Match(_http.GetResponse().Body);
            if (match.Success)
            {
                string json = match.Groups[1].Value;
                uins = EasyJson.ParseJsonObject<List<uins>>(json);
            }
            else
            {
                throw new Exception("获取用户列表失败");
            }
            return uins;
        }
        public QQFaceInfo GetFace(uins uins)
        {
            _http.Open($"https://ssl.ptlogin2.qq.com/getface?appid={_AppId}&imgtype=3&encrytype=0&devtype=0&keytpye=0&uin={uins.uin}&r={common.refreshCode()}", HttpMethod.Get);
            _http.Send();

            var response = _http.GetResponse();
            if (string.IsNullOrEmpty(response.Body))
            {
                throw new Exception("获取头像信息失败：响应为空");
            }

            Regex regex = new Regex(@"pt.setHeader\(([\s\S]+?)\)");
            Match match = regex.Match(response.Body);
            if (match.Success)
            {
                string json = match.Groups[1].Value;
                var faceDict = EasyJson.ParseJsonObject<Dictionary<string, string>>(json);
                if (faceDict.TryGetValue(uins.uin.ToString(), out string url))
                {
                    return new QQFaceInfo
                    {
                        Uin = uins.uin,
                        FaceUrl = url,
                    };
                }
            }
            throw new Exception("获取头像信息失败：未找到对应的头像URL");
        }
        /// <summary>
        /// QQ用户信息类
        /// </summary>
        public class uins
        {
            public long uin { get; set; }
            public int face_index { get; set; }
            public int gender { get; set; }
            public int client_type { get; set; }
            public int uin_flag { get; set; }
            public long account { get; set; }
        }

        /// <summary>
        /// QQ头像信息类
        /// </summary>
        public class QQFaceInfo
        {
            /// <summary>
            /// 用户QQ号
            /// </summary>
            public long Uin { get; set; }

            /// <summary>
            /// 头像URL
            /// </summary>
            public string FaceUrl { get; set; }

            /// <summary>
            /// 是否成功获取头像
            /// </summary>
            public bool IsSuccess => !string.IsNullOrEmpty(FaceUrl);
        }
    }
}
