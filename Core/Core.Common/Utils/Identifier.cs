using System;
using System.Threading;

namespace AFT.RegoV2.Core.Common.Utils
{
    public class Identifier
    {
        private static long _lastTimeStamp = DateTime.UtcNow.Ticks;
        public static DateTimeOffset NewDateTimeOffset()
        {
            const long increment = TimeSpan.TicksPerSecond / 300; // SQL Server is accurate to 1/300th of a second

            long original, newValue;

            do
            {
                original = _lastTimeStamp;
                var now = DateTime.UtcNow.Ticks;
                newValue = Math.Max(now, original + increment);
            } 
            while (Interlocked.CompareExchange(ref _lastTimeStamp, newValue, original) != original);

            return new DateTimeOffset(newValue, TimeSpan.Zero);
        }


        public static Guid NewSequentialGuid()
        {
            var now = NewDateTimeOffset();

            var days = new TimeSpan(now.Ticks - new DateTime(1900, 1, 1).Ticks).Days;
            var msecs = now.TimeOfDay.TotalMilliseconds * 1000 / 300; // SQL Server is accurate to 1/300th of a second

            var daysBytes = BitConverter.GetBytes(days);
            var msecsBytes = BitConverter.GetBytes(msecs);

            // Reverse the bytes to match SQL Servers ordering 
            Array.Reverse(daysBytes);
            Array.Reverse(msecsBytes);

            // Get sequential guid
            var guidBytes = Guid.NewGuid().ToByteArray();
            Array.Copy(daysBytes, daysBytes.Length - 2, guidBytes, guidBytes.Length - 6, 2);
            Array.Copy(msecsBytes, msecsBytes.Length - 4, guidBytes, guidBytes.Length - 4, 4);

            return new Guid(guidBytes);
        }
    }
}
