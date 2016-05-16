using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class BrandIpRegulationDeleted : DomainEventBase
    {
        public BrandIpRegulationDeleted() { } // default constructor is required for publishing event to MQ

        public BrandIpRegulationDeleted(BrandIpRegulation regulation)
        {
            Id = regulation.Id;
        }

        public Guid Id { get; set; }

    }
}
