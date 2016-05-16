using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Brand.Events
{
    public class BrandUpdated : DomainEventBase
    {
        public Guid             Id { get; set; }
        public Guid             LicenseeId { get; set; }
        public string           Code { get; set; }
        public string           Name { get; set; }
        public string           LicenseeName { get; set; }
        public string           TypeName { get; set; }
        public string           Remarks { get; set; }
        public string           PlayerPrefix { get; set; }
        public string           TimeZoneId { get; set; }
        public int              InternalAccountCount { get; set; }
        public DateTimeOffset   DateUpdated { get; set; }
        public string           UpdatedBy { get; set; }
    }
}
