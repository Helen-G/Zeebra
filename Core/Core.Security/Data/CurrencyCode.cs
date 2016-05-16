using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class CurrencyCode
    {
        public Guid UserId { get; set; }

        public string Currency { get; set; }

        public User User { get; set; }
    }
}
