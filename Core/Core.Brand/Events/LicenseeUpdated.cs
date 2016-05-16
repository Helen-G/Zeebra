using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeUpdated : DomainEventBase
    {
        public LicenseeUpdated() { } // default constructor is required for publishing event to MQ

        public LicenseeUpdated(Licensee licensee)
        {
            Id = licensee.Id;
            Name = licensee.Name;
            CompanyName = licensee.CompanyName;
            Email = licensee.Email;
            AffiliateSystem = licensee.AffiliateSystem;
            ContractStart = licensee.ContractStart;
            ContractEnd = licensee.ContractEnd;
            UpdatedBy = licensee.UpdatedBy;
            DateUpdated = licensee.DateUpdated;
            Languages = licensee.Cultures.Select(c => c.Code);
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public bool AffiliateSystem { get; set; }
        public DateTimeOffset ContractStart { get; set; }
        public DateTimeOffset? ContractEnd { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public IEnumerable<string> Languages { get; set; }
    }
}