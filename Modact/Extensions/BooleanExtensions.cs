
namespace Modact
{
    public static partial class BooleanExtensions
    {

        /// <summary>
        /// Get the string as '1' or '0' from boolean into DB
        /// </summary>
        public static string ToStringIntoDb(this bool boolean)
        {
            return (boolean) ? "1" : "0";
        }

        /// <summary>
        /// Get the boolean if string in '1' or '0' from DB
        /// </summary>
        public static bool ToBooleanFromDb(this string str)
        {
            return (str == "1") ? true : false;
        }
    }
}
