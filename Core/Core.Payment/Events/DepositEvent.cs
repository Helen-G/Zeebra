using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Events
{
    public abstract class DepositEvent : DomainEventBase
    {
        protected DepositEvent() { } // default constructor is required for publishing event to MQ

        protected DepositEvent(OfflineDeposit offlineDeposit)
        {
            DepositId = offlineDeposit.Id;
        }

        public Guid DepositId { get; set; }
    }
}
