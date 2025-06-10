namespace Modact
{
    public static partial class ListExtensions
    {
        public static string ToStringSepartor(this List<string> list, string separtor = ",")
        {
            if (list == null) { throw new ArgumentNullException(); }

            var sb = new StringBuilder();
            for (var i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(separtor);
                }
                sb.Append(list[i]);
            }
            return sb.ToString();
        }
    }
}
