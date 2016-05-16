using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class WalletListDTO
    {
        public Guid Brand { get; set; }
        public Guid LicenseeId { get; set; }
        public string BrandName { get; set; }
        public string LicenseeName { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}