using System;
using AFT.RegoV2.Core.Common.Data.Payment;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class PaymentGateway
    {
        public Guid Id { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public BankAccount BankAccount { get; set; }

        public override string ToString()
        {
            return string.Format("Offline - {0} {1}", BankAccount.AccountId, BankAccount.AccountName);
        }
    }
}