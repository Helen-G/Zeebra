﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.WinService.Extensions
{
    public static class Extensions
    {
        public static DateTimeOffset Floor(this DateTimeOffset dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Ceiling(this DateTimeOffset dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(interval.Ticks - (dateTime.Ticks % interval.Ticks));
        }

        public static DateTimeOffset Round(this DateTimeOffset dateTime, TimeSpan interval)
        {
            var halfIntervelTicks = ((interval.Ticks + 1) >> 1);

            return dateTime.AddTicks(halfIntervelTicks - ((dateTime.Ticks + halfIntervelTicks) % interval.Ticks));
        }

        public static int Compare(this Byte[] byteArray1, Byte[] byteArray2)
        {
            for (var i = 0; i < Math.Min(byteArray1.Length, byteArray2.Length); i++)
            {
                if (byteArray1[i] < byteArray2[i])
                {
                    return -1;
                }
                if (byteArray1[i] > byteArray2[i])
                {
                    return 1;
                }
            }
            return
                byteArray1.Length < byteArray2.Length
                    ? -1
                    : byteArray1.Length > byteArray2.Length
                        ? 1
                        : 0;
        }
    }
}
