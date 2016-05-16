using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelStatusUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Status NewStatus { get; set; }

        public RiskLevelStatusUpdated()
        { }

        public RiskLevelStatusUpdated(Guid id, Status newStatus)
        {
            Id = id;
            NewStatus = newStatus;
        }
    }
}
