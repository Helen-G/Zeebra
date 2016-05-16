using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class VipLevelDeactivated : DomainEventBase
    {
        public Guid VipLevelId { get; set; }
    }
}