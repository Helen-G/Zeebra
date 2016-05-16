using System.Text.RegularExpressions;

namespace AFT.RegoV2.Shared
{
    public static class StringExtensions
    {
        public static string SeparateWords(this string combinedString)
        {
            return Regex.Replace(combinedString, "([A-Z])", " $1").Trim();
        }
    }
}
