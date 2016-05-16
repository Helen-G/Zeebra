using System;

namespace AFT.RegoV2.Core.Common.Data.Wallet
{
    public class WalletData
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid WalletTemplateId { get; set; }
        public decimal Total { get; set; }
        public decimal Playable { get; set; }
        public decimal Main { get; set; }
        public decimal Bonus { get; set; }
        public decimal Temporary { get; set; }

        public decimal BonusLock { get; set; }
        public decimal FraudLock { get; set; }
        public decimal WithdrawalLock { get; set; }
    }
}
