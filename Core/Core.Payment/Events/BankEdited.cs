using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class BankEdited : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }

        public BankEdited() { }

        public BankEdited(Bank bank)
        {
            Id = bank.Id;
            Name = bank.Name;
            UpdatedDate = bank.Updated.GetValueOrDefault();
        }
    }
}
