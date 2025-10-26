using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QQLogin
{
    /// <summary>
    /// QQ快速登录
    /// </summary>
    public class QQQuickLogin
    {
        HttpRequestClass _http = new HttpRequestClass();
        readonly string _AppId;
        readonly string _s_url;
        string pt_local_token = "";
        /// <summary>
        /// QQ快速登录
        /// </summary>
        /// <param name="AppId">应用ID</param>
        /// <param name="s_url">登录成功后的跳转地址</param>
        public QQQuickLogin(string AppId = "715030901", string s_url = "https://qun.qq.com/")
        {
            //Edge浏览器UA
            _http.SetUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
            Dictionary<string, string> post_data = new Dictionary<string, string>() {
                {"proxy_url",""},// 登录框代理地址，留空表示使用默认
                {"daid","5"},// 登录框显示模式，5表示新样式
                {"hide_title_bar","1"},// 是否隐藏登录框标题栏，0表示不隐藏，1表示隐藏
                {"low_login","0"},// 是否低版本登录，0表示不低版本登录，1表示低版本登录
                {"qlogin_auto_login","1"},// 是否自动登录，0表示不自动登录，1表示自动登录
                {"no_verifyimg","1"},// 是否禁用验证码登录，0表示不禁用，1表示禁用
                {"link_target","blank"},// 登录成功后的跳转方式，blank表示在新窗口跳转
                {"appid",AppId},// 应用ID，用于标识登录的应用
                {"style","22"},// 登录框样式，22表示新样式
                {"target","self"},// 登录成功后的跳转方式，self表示在当前窗口跳转，blank表示在新窗口跳转
                {"s_url",s_url},// 登录成功后的跳转地址，留空表示使用默认
                {"pt_qr_app",""},// 二维码登录应用名称，留空表示使用默认
                {"pt_qr_link",""},// 二维码登录链接地址，留空表示使用默认
                {"self_regurl",""},// 新用户注册链接地址，留空表示使用默认
                {"pt_qr_help_link",""},// 二维码登录帮助链接地址，留空表示使用默认
                {"pt_no_auth","1"},// 是否禁用无密码登录，0表示不禁用，1表示禁用
            };
            _http.Open($"https://xui.ptlogin2.qq.com/cgi-bin/xlogin?{common.DictionaryToQueryString(post_data)}", HttpMethod.Get);
            _http.Send();
            _AppId = AppId;
            _s_url = s_url;
            pt_local_token = _http.CookieManager().GetCookieValue("pt_local_token");
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
        /// 从pt.setHeader(...)格式的响应中提取JSON部分
        /// </summary>
        private string ExtractJsonFromResponse(string responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
                throw new ArgumentException("响应内容不能为空");

            // 检查是否是pt.setHeader格式
            if (responseContent.TrimStart().StartsWith("pt.setHeader("))
            {
                // 使用正则表达式提取JSON部分
                Regex regex = new Regex(@"pt\.setHeader\((.+)\)\);?", RegexOptions.Singleline);
                Match match = regex.Match(responseContent);

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
                else
                {
                    throw new FormatException("无法从响应中提取JSON部分");
                }
            }

            // 如果不是pt.setHeader格式，直接返回（可能已经是纯JSON）
            return responseContent;
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
