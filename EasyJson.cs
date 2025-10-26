using System.Dynamic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EasyJson
{
    public static dynamic ParseJsonToDynamic(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 字符串不能为空或空白");

        using JsonDocument doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 将 JsonElement 转换为动态对象
        return ConvertToDynamic(root);
    }

    /// <summary>
    /// 验证字符串是否为有效的JSON格式
    /// </summary>
    /// <param name="jsonString">需要验证的字符串</param>
    /// <returns>如果是有效JSON返回true，否则返回false</returns>
    public static bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return false;

        try
        {
            // 尝试解析JSON字符串
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                // 如果解析成功，返回true
                return true;
            }
        }
        catch (JsonException)
        {
            // 如果解析失败，返回false
            return false;
        }
        catch (Exception)
        {
            // 捕获其他可能的异常
            return false;
        }
    }

    private static dynamic ConvertToDynamic(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ParseObject(element),
            JsonValueKind.Array => ParseArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => ParseNumber(element),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private static IDictionary<string, object> ParseObject(JsonElement element)
    {
        var result = new ExpandoObject() as IDictionary<string, object>;
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ConvertToDynamic(property.Value);
        }
        return result;
    }

    private static List<object> ParseArray(JsonElement element)
    {
        var result = new List<object>();
        foreach (var item in element.EnumerateArray())
        {
            result.Add(ConvertToDynamic(item));
        }
        return result;
    }

    private static object ParseNumber(JsonElement element)
    {
        // 自动检测最佳数字类型
        if (element.TryGetInt32(out int i)) return i;
        if (element.TryGetInt64(out long l)) return l;
        if (element.TryGetDouble(out double d)) return d;
        if (element.TryGetDecimal(out decimal dec)) return dec;
        return element.GetRawText(); // 回退字符串
    }
    /// <summary>
    /// 解析 JSON 字符串为指定类型的数组
    /// </summary>
    /// <typeparam name="T">数组元素类型 (int, string, bool, 自定义类等)</typeparam>
    /// <param name="json">有效的 JSON 数组字符串</param>
    /// <returns>解析后的数组</returns>
    public static T[] ParseJsonArray<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 字符串不能为空或空白");

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                // 关键修复：处理数字转字符串等类型转换
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                // 处理空值
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };

            var result = JsonSerializer.Deserialize<T[]>(json, options);

            return result ?? throw new InvalidOperationException("解析结果不能为null");
        }
        catch (JsonException ex)
        {
            // 添加详细错误信息
            throw new FormatException($"JSON解析错误 (目标类型={typeof(T).Name}): {ex.Message}\nJSON内容: {TruncateJson(json)}", ex);
        }
    }
    /// <summary>
    /// 解析 JSON 到自定义类型对象
    /// </summary>
    /// <typeparam name="T">目标对象类型</typeparam>
    /// <param name="json">JSON 字符串</param>
    public static T ParseJsonObject<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON 字符串不能为空或空白");

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreReadOnlyProperties = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            // 添加DateTime自定义转换器
            options.Converters.Add(new DateTimeConverter());
            // 如果存在DateTimeOffset也添加支持
            options.Converters.Add(new DateTimeOffsetConverter());

            var result = JsonSerializer.Deserialize<T>(json, options);
            return result ?? throw new InvalidOperationException("解析结果不能为null");
        }
        catch (JsonException ex)
        {
            throw new FormatException($"JSON解析错误 (目标类型={typeof(T).Name}): {ex.Message}\nJSON内容: {TruncateJson(json)}", ex);
        }
    }

    // 自定义DateTime转换器 (支持ISO 8601、时间戳、自定义格式)
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                // 处理ISO 8601字符串
                if (reader.TokenType == JsonTokenType.String)
                {
                    string dateString = reader.GetString()!;
                    return DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
                // 处理Unix时间戳（毫秒）
                else if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long milliseconds))
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
                }
                // 处理其他格式
                else if (reader.TokenType == JsonTokenType.String)
                {
                    string dateString = reader.GetString()!;
                    return DateTime.Parse(dateString);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException($"DateTime解析失败: {ex.Message}", ex);
            }

            throw new JsonException("DateTime格式异常");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O")); // ISO 8601格式
        }
    }

    // DateTimeOffset转换器 (与DateTime逻辑类似)
    public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64());
            }
            throw new JsonException("DateTimeOffset格式异常");
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }

    // JSON截断工具函数
    private static string TruncateJson(string json, int maxLength = 200)
    {
        return json.Length <= maxLength ? json : json.Substring(0, maxLength) + "...(已截断)";
    }
    /// <summary>
    /// 动态解析未知类型的 JSON 数组
    /// </summary>
    public static List<object> ParseAnyJsonArray(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("输入的 JSON 不是数组格式");

        var result = new List<object>();

        foreach (var element in root.EnumerateArray())
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    result.Add(element.GetString());
                    break;
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        result.Add(intValue);
                    else if (element.TryGetDouble(out double doubleValue))
                        result.Add(doubleValue);
                    else
                        result.Add(element.GetRawText()); // 其他数字类型
                    break;
                case JsonValueKind.True:
                    result.Add(true);
                    break;
                case JsonValueKind.False:
                    result.Add(false);
                    break;
                case JsonValueKind.Array:
                    result.Add(ParseAnyJsonArray(element.GetRawText()));
                    break;
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict.Add(prop.Name, GetJsonValue(prop.Value));
                    }
                    result.Add(dict);
                    break;
                default:
                    result.Add(null); // 空值处理
                    break;
            }
        }

        return result;
    }

    private static object GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int intVal)
                    ? (object)intVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => ParseAnyJsonArray(element.GetRawText()),
            JsonValueKind.Object =>
                element.Deserialize<Dictionary<string, object>>(),
            _ => null
        };
    }
}
