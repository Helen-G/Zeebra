using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class BrandDefaultVipLevelChanged : DomainEventBase
    {
        public Guid     BrandId { get; set; }
        public Guid?    OldVipLevelId { get; set; }
        public Guid     DefaultVipLevelId { get; set; }
    }
}