using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APM.UtilEntities
{
    public class JsonDateTimeConverter(string format) : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return DateTime.MinValue;
            }

            // 1. 首先尝试按照你指定的 yyyy-MM-dd HH:mm:ss 格式解析
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                return dateTime;
            }

            // 2. 如果失败（比如遇到了带 T 的 UTC 格式），则尝试使用系统默认的解析方式
            // 它可以自动处理 ISO-8601 格式，如 2026-05-04T02:13:50.2298667
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var fallbackDateTime))
            {
                return fallbackDateTime;
            }

            throw new JsonException($"无法将日期字符串 '{dateString}' 转换为 DateTime。预期格式为：{format} 或标准 ISO 格式。");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format));
        }
    }
}
