using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public bool AffiliateSystem { get; set; }
        public DateTimeOffset ContractStart { get; set; }
        public DateTimeOffset? ContractEnd { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public IEnumerable<string> Languages { get; set; }
    }
}