using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class BrandActivated : DomainEventBase
    {
        public BrandActivated() { } // default constructor is required for publishing event to MQ

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TimezoneId { get; set; }
        public Guid LicenseeId { get; set; }
        public string LicenseeName { get; set; }

        public Dictionary<string, string> Currencies { get; set; }
        public Dictionary<string, string> Countries { get; set; }
        public Dictionary<string, string> VipLevels { get; set; }
        public List<WalletTemplateData> WalletTemplates { get; set; }

        public DateTimeOffset DateActivated { get; set; }
        public string ActivatedBy { get; set; }
    }

    public class WalletTemplateData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> ProductIds { get; set; }
    }
}