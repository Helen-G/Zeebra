using System;

namespace AFT.RegoV2.Core.Brand.Data
{
    /// <summary>
    /// Cross-reference table connecting VipLevel with BetLimit entity.
    /// This connection is valid in the context of specified GameProvider and Currency entities.
    /// </summary>
    public class VipLevelGameProviderBetLimit
    {
        public Guid     Id { get; set; }
        public Guid     VipLevelId { get; set; }
        public Guid     BetLimitId { get; set; }

        public Guid     GameProviderId { get; set; }
        public string   CurrencyCode { get; set; }

        public VipLevel VipLevel { get; set; }
        public Currency Currency { get; set; }
    }
}