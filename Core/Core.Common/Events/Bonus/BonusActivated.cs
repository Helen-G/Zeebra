using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Bonus
{
    public class BonusActivated : DomainEventBase
    {
        public BonusActivated() { }

        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}
