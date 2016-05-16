using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class BrandProductData
    {
        public string   BrandName { get; set; }
        public string   GameProviderName { get; set; }
        public Guid     BrandId { get; set; }
        public Guid     GameProviderId { get; set; }
        public Guid     LicenseeId { get; set; }
        public string   UpdatedBy { get; set; }
        public string   CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}