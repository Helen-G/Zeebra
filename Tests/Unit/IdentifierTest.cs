using System;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Event
{
    internal class IdentifierTest : AdminWebsiteUnitTestsBase
    {
        [Test, Explicit]
        public void Can_generate_hundred_different_datetimeoffsets_with_error_less_than_second()
        {
            DateTimeOffset oldDateTimeOffset = DateTimeOffset.MinValue;
            for (var i = 0; i < 100; i++)
            {
                var dateTimeOffset = Identifier.NewDateTimeOffset();
                Assert.AreNotEqual(oldDateTimeOffset, dateTimeOffset);
                oldDateTimeOffset = dateTimeOffset;
            }
            Assert.GreaterOrEqual(1000, Math.Abs((oldDateTimeOffset - DateTimeOffset.UtcNow).TotalMilliseconds));
        }

        [Test, Explicit]
        public void Can_generate_hundred_different_sequential_guids()
        {
            var oldGuid = new Guid();
            var oldGuidBytes = oldGuid.ToByteArray();
            for (var i = 0; i < 100; i++)
            {
                var guid = Identifier.NewSequentialGuid();
                var guidBytes = guid.ToByteArray();
                if (i != 0)
                {
                    Assert.AreNotEqual(oldGuid, guid);
                    Assert.AreEqual(oldGuidBytes[10], guidBytes[10]);
                }
                oldGuid = guid;
                oldGuidBytes = guidBytes;
            }
        }
    }
}
