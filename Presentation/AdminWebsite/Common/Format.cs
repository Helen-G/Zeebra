using System;
using System.Globalization;

namespace AFT.RegoV2.AdminWebsite.Common
{
    public class Format
    {
        public static string FormatDate(DateTimeOffset? dto, bool includeTime = true)
        {
            var dateFormat = includeTime ? "yyyy/MM/dd HH:mm:ss" : "yyyy/MM/dd";
            return dto.HasValue ? dto.Value.ToString(dateFormat, CultureInfo.InvariantCulture) : string.Empty;
        }

        public static DateTimeOffset? FormatDateString(string date)
        {
            if (string.IsNullOrEmpty(date)) return null;

            var dto = DateTimeOffset.ParseExact(date, "yyyy/MM/dd", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal);

            return dto;
        }
    }
}