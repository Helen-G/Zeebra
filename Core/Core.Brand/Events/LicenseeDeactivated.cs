using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeDeactivated : DomainEventBase
    {
        public LicenseeDeactivated() { } // default constructor is required for publishing event to MQ

        public LicenseeDeactivated(Licensee licensee)
        {
            Id = licensee.Id;
            DeactivatedBy = licensee.DeactivatedBy;
            DateDeactivated = licensee.DateDeactivated.Value;
            Remarks = licensee.Remarks;
        }

        public Guid Id { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DateDeactivated { get; set; }
        public string Remarks { get; set; }
    }
}