using System;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Brand.Events
{
    public class BrandRegistered : DomainEventBase
    {
        public Guid             Id { get; set; }
        public string           Code { get; set; }
        public string           Name { get; set; }
        public Guid             LicenseeId { get; set; }
        public string           LicenseeName { get; set; }
        public string           TimeZoneId { get; set; }
        public BrandType        BrandType { get; set; }
        public BrandStatus      Status { get; set; }
        public string           PlayerPrefix { get; set; }
        public int              InternalAccountsNumber { get; set; }
        public DateTimeOffset   DateCreated { get; set; }
        public string           CreatedBy { get; set; }
    }
}