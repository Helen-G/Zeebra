using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;

namespace AFT.RegoV2.Domain.Security.Events
{
    public class AdminIpRegulationCreated : DomainEventBase
    {
        public AdminIpRegulationCreated() { } // default constructor is required for publishing event to MQ

        public AdminIpRegulationCreated(AdminIpRegulation regulation)
        {
            Id = regulation.Id;
            IpAddress = regulation.IpAddress;
            Description = regulation.Description;
        }

        public Guid Id { get; set; }

        public string IpAddress { get; set; }

        public string Description { get; set; }

        public string RedirectionUrl { get; set; }

        public string BlockingType { get; set; }

    }
}
