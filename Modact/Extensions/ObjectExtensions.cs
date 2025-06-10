using System.Runtime.Serialization;
using System.Xml;

namespace Modact
{
    public static partial class ObjectExtensions
    {
        public static string? ToStringJson(this object obj, string datatimeStringFormat = null)
        {
            if (obj == null) { return null; }
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, //json ignore the null property variables
                PropertyNamingPolicy = null, //json keep char letter case

            };
            if (!string.IsNullOrEmpty(datatimeStringFormat))
            {
                options.Converters.Add(new DateTimeJsonConverter(datatimeStringFormat)); //json datetime format
            }
            options.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Serialize(obj, options);
        }

        public static T DeserializeTo<T>(this object obj)
        {
            if (obj == null)
            {
                return default;
            }
            if (obj is JsonElement)
            {
                return ((JsonElement)obj).Deserialize<T>();
            }
            if (obj is JsonDocument)
            {
                return ((JsonDocument)obj).Deserialize<T>();
            }
            if (obj is string)
            {
                return JsonSerializer.Deserialize<T>((string)obj);
            }
            if (obj is byte[])
            {
                return ((byte[])obj).DeserializeTo<T>();
            }

            return default;
        }

        // Convert an object to a byte array
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            DataContractSerializer ser = new DataContractSerializer(typeof(object));
            ser.WriteObject(memoryStream, obj);
            var data = memoryStream.ToArray();
            return data;
        }

        // Convert a byte array to an Object
        public static T DeserializeTo<T>(this byte[] data)
        {
            if (data == null)
            {
                return default;
            }

            using var memoryStream = new MemoryStream(data);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
            var ser = new DataContractSerializer(typeof(T));
            var result = (T)ser.ReadObject(reader, true);
            return result;
        }

        public static bool IsGenericList(this object o)
        {
            var oType = o.GetType();
            return oType.GetTypeInfo().IsGenericType && oType.GetGenericTypeDefinition() == typeof(List<>);
        }
    }

}
