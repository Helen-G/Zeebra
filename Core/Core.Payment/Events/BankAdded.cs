using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class BankAdded : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public BankAdded() { }

        public BankAdded(Bank bank)
        {
            Id = bank.Id;
            Name = bank.Name;
            CreatedDate = bank.Created;
        }
    }
}
