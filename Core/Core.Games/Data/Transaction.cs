using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Transaction
    {
        public Transaction()
        {
            Id = Guid.NewGuid();
        }

        internal Transaction(Wallet wallet): this()
        {
            WalletId = wallet.Id;
            MainBalance = wallet.Main;
            BonusBalance = wallet.Bonus;
            TemporaryBalance = wallet.Temporary;
            CreatedOn = DateTimeOffset.Now.ToBrandOffset(wallet.Brand.TimezoneId);
        }

        public Guid Id { get; set; }
        public Guid? RoundId { get; set; }
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

        [Required]
        public string Description { get; set; }
        public string TransactionNumber { get; set; }

        public Guid? PerformedBy { get; set; }
    }
}