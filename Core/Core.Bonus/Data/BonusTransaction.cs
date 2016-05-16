using System;
using AFT.RegoV2.Core.Common.Data.Wallet;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class BonusTransaction : Identity
    {
        public Guid? RoundId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal MainBalanceAmount { get; set; }
        public decimal BonusBalanceAmount { get; set; }
        public TransactionType Type { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}