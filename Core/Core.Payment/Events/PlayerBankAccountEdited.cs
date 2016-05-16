using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PlayerBankAccountEdited : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public PlayerBankAccountEdited()
        {
        }

        public PlayerBankAccountEdited(PlayerBankAccount bankAccount)
        {
            Id = bankAccount.Id;
            Name = bankAccount.AccountName;
            Number = bankAccount.AccountNumber;
            UpdatedBy = bankAccount.UpdatedBy;
            UpdatedDate = bankAccount.Updated.GetValueOrDefault();
        }
    }
}
