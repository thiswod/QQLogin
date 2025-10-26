using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace QQLogin
{
    public class common
    {
        #region URL参数相关方法
        /// <summary>
        /// 将 URL 参数按 ASCII 码排序（区分大小写）
        /// </summary>
        /// <param name="queryString">URL 参数字符串（格式 key1=value1&key2=value2）</param>
        /// <param name="encodeValue">是否对值进行 URL 编码（默认不编码）</param>
        /// <returns>按 ASCII 排序后的参数字符串</returns>
        public static string SortUrlParameters(string queryString, bool encodeValue = false)
        {
            if (string.IsNullOrWhiteSpace(queryString))
                return string.Empty;

            // 拆分成键值对
            var parameters = new Dictionary<string, string>();
            var keyValuePairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in keyValuePairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 0) continue;

                string key = kv[0];
                string value = kv.Length > 1 ? kv[1] : "";

                // 仅保留第一个参数值（可选）
                if (!parameters.ContainsKey(key))
                {
                    // 选择性进行 URL 编码
                    value = encodeValue ? WebUtility.UrlEncode(value) : value;
                    parameters.Add(key, value);
                }
            }

            // 按 ASCII 顺序排序（区分大小写）
            var sorted = parameters
                .OrderBy(kv => kv.Key, StringComparer.Ordinal) // ASCII 顺序
                .ToArray();

            // 构建排序后的字符串
            var sb = new StringBuilder();
            foreach (var (key, value) in sorted)
            {
                if (sb.Length > 0) sb.Append('&');
                sb.Append(key).Append('=').Append(value);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 对整个 URL 进行参数排序（保留协议和域名部分）
        /// </summary>
        public static string SortUrlParametersInFullUrl(string url, bool encodeValues = false)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("无效的 URL");

            // 分离基本 URL 和参数部分
            var baseUri = uri.GetLeftPart(UriPartial.Path);
            var query = uri.Query.TrimStart('?');

            // 处理片段部分（锚点）
            string fragment = string.IsNullOrEmpty(uri.Fragment)
                ? "" : "#" + uri.Fragment.TrimStart('#');

            // 排序参数
            string sortedQuery = SortUrlParameters(query, encodeValues);

            return baseUri + (string.IsNullOrEmpty(sortedQuery)
                ? "" : "?" + sortedQuery) + fragment;
        }
        /// <summary>
        /// 将查询字符串转换为字典
        /// </summary>
        /// <param name="queryString">查询字符串，格式为key1=value1&key2=value2</param>
        /// <returns>包含键值对的字典</returns>
        public static Dictionary<string, string> QueryStringToDictionary(string queryString)
        {
            var parameters = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(queryString))
                return parameters;

            // 移除可能的问号前缀
            if (queryString.StartsWith('?'))
                queryString = queryString.Substring(1);

            var keyValuePairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in keyValuePairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length == 0) continue;

                string key = kv[0];
                string value = kv.Length > 1 ? kv[1] : "";

                // URL解码值
                value = WebUtility.UrlDecode(value);

                // 如果键已存在，则覆盖
                if (parameters.ContainsKey(key))
                    parameters[key] = value;
                else
                    parameters.Add(key, value);
            }

            return parameters;
        }
        /// <summary>
        /// 将字典转换为查询字符串
        /// </summary>
        /// <param name="dictionary">包含键值对的字典</param>
        /// <param name="encodeValue">是否对值进行URL编码（默认编码）</param>
        /// <returns>格式化的查询字符串，格式为key1=value1&key2=value2</returns>
        public static string DictionaryToQueryString(Dictionary<string, string> dictionary, bool encodeValue = true)
        {
            if (dictionary == null || dictionary.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var kvp in dictionary)
            {
                if (sb.Length > 0)
                    sb.Append('&');

                sb.Append(kvp.Key);
                sb.Append('=');

                // 根据参数决定是否对值进行URL编码
                string value = encodeValue ? WebUtility.UrlEncode(kvp.Value ?? "") : (kvp.Value ?? "");
                sb.Append(value);
            }

            return sb.ToString();
        }
        #endregion
        private static readonly Random _random = new Random();
        /// <summary>
        /// 生成随机时间戳
        /// </summary>
        public static string refreshCode()
        {
            double refreshCode = _random.NextDouble();
            return $"{refreshCode:F16}";
        }
    }
}
