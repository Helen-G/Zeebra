using System;
using AFT.RegoV2.Core.Common.Data.Wallet;

namespace AFT.RegoV2.Core.Wallet.Data
{
    public class Transaction
    {
        public Transaction()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.Now;
        }

        internal Transaction(Wallet wallet): this()
        {
            WalletId = wallet.Id;
            MainBalance = wallet.Main;
            BonusBalance = wallet.Bonus;
            TemporaryBalance = wallet.Temporary;
        }

        public Guid Id { get; set; }
        public Guid? BetId { get; set; }
        public Guid? GameId { get; set; }
        public TransactionType Type { get; set; }
        public decimal MainBalanceAmount { get; set; }
        public decimal BonusBalanceAmount { get; set; }
        public decimal TemporaryBalanceAmount { get; set; }

        public decimal MainBalance { get; set; }
        public decimal BonusBalance { get; set; }
        public decimal TemporaryBalance { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public Guid WalletId { get; set; }
        public Guid? RelatedTransactionId { get; set; }

        public string Description { get; set; }
        public string TransactionNumber { get; set; }

        public Guid? PerformedBy { get; set; }
    }
}