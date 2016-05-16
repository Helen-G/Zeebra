using System;

namespace AFT.RegoV2.Core.Common.Utils
{
    public static class SystemTime
    {
        public static Func<DateTimeOffset> Factory = () => DateTimeOffset.Now;

        public static DateTimeOffset Now
        {
            get
            {
                return Factory(); 
            }
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static string GetNormalizedDateTimeWithOffset(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd h:mm:ss tt zzz");
        }

        public static string GetNormalizedDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd h:mm:ss");
        }

        public static string GetNormalizedDate(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd");
        }

        public static DateTimeOffset ToBrandDateTimeOffset(this DateTime dt, string brandTimezoneId)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(brandTimezoneId);

            return new DateTimeOffset(dt, timeZoneInfo.GetUtcOffset(dt));
        }
    }

    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ToBrandOffset(this DateTimeOffset dateTimeOffset, string brandTimezoneId)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTimeOffset, brandTimezoneId);
        }
    }
}