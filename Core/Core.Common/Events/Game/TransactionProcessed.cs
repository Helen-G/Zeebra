using System;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Wallet
{
    public class TransactionProcessed : DomainEventBase
    {
        public TransactionProcessed() { } // default constructor is required for publishing event to MQ

        public WalletData       Wallet { get; set; }
        public Guid             TransactionId { get; set; }
        public Guid?            RelatedTransactionId { get; set; }
        public decimal          MainBalanceAmount { get; set; }
        public decimal          BonusBalanceAmount { get; set; }
        public decimal          TemporaryBalanceAmount { get; set; }
        public TransactionType  PaymentType { get; set; }
        public Guid?            RoundId { get; set; }
        public Guid?            GameId { get; set; }
        public DateTimeOffset   CreatedOn { get; set; }
        public string           Description { get; set; }
        public string           TransactionNumber { get; set; }
        public Guid?            PerformedBy { get; set; }
    }
}