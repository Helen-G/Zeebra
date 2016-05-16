using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class BankAccountActivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTimeOffset ActivatedDate { get; set; }
        public string ActivatedBy { get; set; }

        public BankAccountActivated()
        {
        }

        public BankAccountActivated(BankAccount bankAccount)
        {
            Id = bankAccount.Id;
            Name = bankAccount.AccountName;
            Number = bankAccount.AccountNumber;
            ActivatedBy = bankAccount.UpdatedBy;
            ActivatedDate = bankAccount.Updated.GetValueOrDefault();
        }
    }
}
