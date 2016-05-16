using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Bonus
{
    public class BonusDeactivated : DomainEventBase
    {
        public BonusDeactivated() { }

        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}
