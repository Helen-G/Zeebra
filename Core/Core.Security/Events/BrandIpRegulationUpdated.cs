using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class BrandIpRegulationUpdated : DomainEventBase
    {
        public BrandIpRegulationUpdated() { } // default constructor is required for publishing event to MQ

        public BrandIpRegulationUpdated(BrandIpRegulation regulation)
        {
            Id = regulation.Id;
            LicenseeId = regulation.LicenseeId;
            BrandId = regulation.BrandId;
            IpAddress = regulation.IpAddress;
            Description = regulation.Description;
            RedirectionUrl = regulation.RedirectionUrl;
            BlockingType = regulation.BlockingType;
        }

        public Guid Id { get; set; }

        public Guid LicenseeId { get; set; }

        public Guid BrandId { get; set; }

        public string IpAddress { get; set; }

        public string Description { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }

    }
}
