using System;

namespace Modact
{
    public static partial class StringExtensions
    {
        public static string Base64Encode(this string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }
        public static string Base64Decode(this string value)
        {
            value = value.Replace('_', '/').Replace('-', '+');
            switch (value.Length % 4)
            {
                case 2: value += "=="; break;
                case 3: value += "="; break;
            }
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }

        public static T JsonDeserialize<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            { return default; }

            return JsonSerializer.Deserialize<T>(value);
        }
        public static string If(this string str, bool condition)
        {
            return condition ? str : string.Empty;
        }

        public static string? SubstringMax(this string? str, int maxLength)
        {
            if (str == null) { return str; }

            if (str.Length <= maxLength) { return str; }

            return str.Substring(0, maxLength);
        }

        public static bool ModactConfigAsBOOL(this string? str)
        {
            if (str == "1" || str.ToUpper() == "TRUE" || str.ToUpper() == "Y") { return true; }

            if (str == "0" || str.ToUpper() == "FALSE" || str.ToUpper() == "N") { return false; }

            throw new InvalidCastException($"Cannot cast config value '{str}' to boolean.");
        }
        public static DateTime ModactConfigAsDATETIME2(this string? str)
        {
            return DateTime.ParseExact(str, "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static decimal ModactConfigAsDECIMAL(this string? str)
        {
            return decimal.Parse(str);
        }
        public static int ModactConfigAsINT(this string? str)
        {
            return int.Parse(str);
        }

        public static string Repeat(this string? value, int repeatCount)
        {
            return string.Concat(Enumerable.Repeat(value, repeatCount));
        }
    }
}
