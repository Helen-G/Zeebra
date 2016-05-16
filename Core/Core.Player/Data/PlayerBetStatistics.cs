using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class PlayerBetStatistics
    {
        public Guid PlayerId { get; set; }
        public decimal TotalLoss { get; set; }
        public decimal TotalWon { get; set; }
        public decimal TotlAdjusted { get; set; }
    }
}