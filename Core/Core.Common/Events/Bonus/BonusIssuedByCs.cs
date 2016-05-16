using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Bonus
{
    public class BonusIssuedByCs : DomainEventBase
    {
        public BonusIssuedByCs() { }

        public Guid BonusId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid TransactionId { get; set; }
        public string Description { get; set; }
    }
}
