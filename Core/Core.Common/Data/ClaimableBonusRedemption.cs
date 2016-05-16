using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class ClaimableBonusRedemption
    {
        public Guid Id { get; set; }
        public string BonusName { get; set; }
        public decimal RewardAmount { get; set; }
        public DateTimeOffset ClaimableFrom { get; set; }
        public DateTimeOffset ClaimableTo { get; set; }
    }
}
