using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class Brand
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid LicenseeId { get; set; }
        public string LicenseeName { get; set; }
        public string TimeZoneId { get; set; }
        public virtual ICollection<AutoVerificationCheckConfiguration> AutoVerificationCheckConfigurations { get; set; }
    }
}
