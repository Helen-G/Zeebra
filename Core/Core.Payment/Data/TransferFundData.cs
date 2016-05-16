using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class TransferFund
    {
        public Guid Id { get; set; }

        public string TransactionNumber { get; set; }

        public TransferFundType TransferType { get; set; }

        public string WalletId { get; set; }

        public decimal Amount { get; set; }

        public TransferFundStatus Status { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public string Remarks { get; set; }

        public string BonusCode { get; set; }
        public Guid DestinationWalletId { get; set; }
    }
}
