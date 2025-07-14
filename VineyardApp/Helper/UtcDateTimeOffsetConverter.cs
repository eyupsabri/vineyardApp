using System.Text.Json;
using System.Text.Json.Serialization;

namespace VineyardApp.Helper
{
    public class UtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        // 2) Reading: if the JSON token is null, return null; otherwise parse the string back into a DateTimeOffset.
        public override DateTimeOffset? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            // Safe built-in parser for ISO-8601 strings (with any offset or trailing Z)
            return DateTimeOffset.Parse(reader.GetString()!);
        }

        // 3) Writing: if our value is null, emit JSON null; otherwise...
        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset? value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // 4) Convert the timestamp to UTC (so e.g. 16:31+03:00 → 13:31Z)
            DateTimeOffset utc = value.Value.ToUniversalTime();

            // 5) Format the UTC value with exactly 7 fractional digits, then a trailing 'Z'
            //    yyyy-MM-ddTHH:mm:ss.fffffffZ
            writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
        }
    }
}
