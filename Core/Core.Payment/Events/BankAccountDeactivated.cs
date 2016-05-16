using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class BankAccountDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }
        public string DeactivatedBy { get; set; }

        public BankAccountDeactivated()
        {
        }

        public BankAccountDeactivated(BankAccount bankAccount)
        {
            Id = bankAccount.Id;
            Name = bankAccount.AccountName;
            Number = bankAccount.AccountNumber;
            DeactivatedBy = bankAccount.UpdatedBy;
            DeactivatedDate = bankAccount.Updated.GetValueOrDefault();
        }
    }
}
