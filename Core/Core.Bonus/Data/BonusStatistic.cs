namespace AFT.RegoV2.Core.Bonus.Data
{
    public class BonusStatistic: Identity
    {
        /// <summary>
        /// Total bonus amount issued to players across all bonus versions
        /// </summary>
        public decimal TotalRedeemedAmount { get; set; }
        /// <summary>
        /// Number of bonus redemption across all bonus versions
        /// </summary>
        public int TotalRedemptionCount { get; set; }
    }
}