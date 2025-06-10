namespace Modact
{
    public static partial class JsonExtensions
    {
        public static dynamic GetDynamicObject(this JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new System.Dynamic.ExpandoObject();
                    foreach (var property in element.EnumerateObject())
                    {
                        ((System.Collections.Generic.IDictionary<string, object>)obj)[property.Name] = GetDynamicObject(property.Value);
                    }
                    return obj;
                case JsonValueKind.Array:
                    var array = new System.Collections.Generic.List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        array.Add(GetDynamicObject(item));
                    }
                    return array;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    return element.GetDecimal();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                case JsonValueKind.Null:
                    return null;
                default:
                    throw new NotSupportedException($"Unsupported value kind: {element.ValueKind}");
            }
        }
    }
}
