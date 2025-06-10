namespace Modact
{
    public static partial class DateTimeExtensions
    {
        public static DateTime MinValue(this DateTime currentDate)
        {
            return new DateTime(1900, 1, 1);
        }

        public static DateTime MaxValue(this DateTime currentDate)
        {
            return new DateTime(9999, 12, 31);
        }

        public static DateTime GetFirstDateOfMonth(this DateTime currentDate)
        {
            return currentDate.AddDays((-1) * currentDate.Day + 1);
        }

        public static DateTime GetLastDateOfMonth(this DateTime currentDate)
        {
            return currentDate.AddMonths(1).GetFirstDateOfMonth().AddDays(-1);
        }

        public static DateTime[] GetDatesArray(this DateTime fromDate, DateTime toDate)
        {
            int days = (toDate - fromDate).Days;
            var dates = new DateTime[days];

            for (int i = 0; i < days; i++)
            {
                dates[i] = fromDate.AddDays(i);
            }

            return dates;
        }

        public static DateTime[] GetCalendarMonthDatesArray(this DateTime currentDate)
        {
            var first = currentDate.GetFirstDateOfMonth();
            var firstWeekDay = (int)first.DayOfWeek;
            if (firstWeekDay == 0)
                firstWeekDay = 7;

            var from = first.AddDays((int)DayOfWeek.Monday - firstWeekDay);
            var last = currentDate.GetLastDateOfMonth();
            var lastWeekDay = (int)last.DayOfWeek;
            if (lastWeekDay == 0)
                lastWeekDay = 7;

            var thru = last.AddDays(lastWeekDay - (int)DayOfWeek.Monday + 1);
            return from.GetDatesArray(thru);
        }

        public static long ToUnixTimestamp(this DateTime date)
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
