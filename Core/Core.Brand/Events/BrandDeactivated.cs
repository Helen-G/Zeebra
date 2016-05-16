using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class BrandDeactivated : DomainEventBase
    {
        public BrandDeactivated() { } // default constructor is required for publishing event to MQ

        public BrandDeactivated(Data.Brand brand)
        {
            Id = brand.Id;
            TimeZoneId = brand.TimezoneId;
            DateDeactivated = brand.DateDeactivated.Value;
            DeactivatedBy = brand.DeactivatedBy;
        }

        public Guid Id { get; set; }
        public string TimeZoneId { get; set; }
        public DateTimeOffset DateDeactivated { get; set; }
        public string DeactivatedBy { get; set; }
    }
}
