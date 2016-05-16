using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PlayerBankAccountAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public PlayerBankAccountAdded()
        {
        }

        public PlayerBankAccountAdded(PlayerBankAccount bankAccount)
        {
            Id = bankAccount.Id;
            Name = bankAccount.AccountName;
            Number = bankAccount.AccountNumber;
            CreatedBy = bankAccount.CreatedBy;
            CreatedDate = bankAccount.Created;
        }
    }
}
