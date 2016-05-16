using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Payment
{
    public class DepositSubmitted : DomainEventBase
    {
        public DepositSubmitted()
        {
        }

        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Submitted { get; set; }
        public string SubmittedBy { get; set; }
        public string CurrencyCode { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountId { get; set; }
        public string BankProvince { get; set; }
        public string BankBranch { get; set; }
        public string BankName { get; set; }
        public string Remarks { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DepositType DepositType { get; set; }
        public string BonusCode { get; set; }
    }
}
