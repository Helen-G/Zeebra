using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class TokenData
    {
        public long Time { get; set; }
        public Guid TokenId { get; set; }
        public Guid PlayerId { get; set; }

        public Guid GameId { get; set; }
        public Guid BrandId { get; set; }

        public string CurrencyCode { get; set; }
        public string PlayerIpAddress { get; set; }
    }
}