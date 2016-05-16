using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class BrandProductsAssigned : DomainEventBase
    {
        public Guid     BrandId { get; set; }
        public Guid[]   ProductsIds { get; set; }
    }
}
