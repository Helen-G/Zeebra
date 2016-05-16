using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeCreated : DomainEventBase
    {
        public LicenseeCreated() { } // default constructor is required for publishing event to MQ

        public LicenseeCreated(Licensee licensee)
        {
            Id = licensee.Id;
            Name = licensee.Name;
            CompanyName = licensee.CompanyName;
            Email = licensee.Email;
            AffiliateSystem = licensee.AffiliateSystem;
            ContractStart = licensee.ContractStart;
            ContractEnd = licensee.ContractEnd;
            CreatedBy = licensee.CreatedBy;
            DateCreated = licensee.DateCreated;
            Languages = licensee.Cultures.Select(c => c.Code);
        }

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