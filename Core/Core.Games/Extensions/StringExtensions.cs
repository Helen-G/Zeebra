using System;

namespace AFT.RegoV2.Core.Game.Extensions
{
    public static class StringExtensions
    {
        public static string Args(this string str, params object[] args)
        {
            return String.Format(str, args);
        }
    }
}