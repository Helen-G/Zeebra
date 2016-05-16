using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LicenseeActivated : DomainEventBase
    {
        public LicenseeActivated() { } // default constructor is required for publishing event to MQ

        public LicenseeActivated(Licensee licensee)
        {
            Id = licensee.Id;
            ActivatedBy = licensee.ActivatedBy;
            DateActivated = licensee.DateActivated.Value;
            Remarks = licensee.Remarks;
        }

        public Guid Id { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset DateActivated { get; set; }
        public string Remarks { get; set; }
    }
}