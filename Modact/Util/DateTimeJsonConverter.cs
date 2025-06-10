using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modact
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private string _datetimeStringFormat;

        public DateTimeJsonConverter(string datetimeStringFormat)
        {
            _datetimeStringFormat = datetimeStringFormat;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out DateTime date))
                {
                    return date;
                }
            }
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_datetimeStringFormat));
        }
    }

}
