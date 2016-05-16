using System;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class EditLicenseeData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public bool AffiliateSystem { get; set; }
        public DateTimeOffset ContractStart { get; set; }
        public DateTimeOffset? ContractEnd { get; set; }
        public bool OpenEnded { get; set; }
        public string Email { get; set; }
        public string TimeZoneId { get; set; }
        public int BrandCount { get; set; }
        public int WebsiteCount { get; set; }
        public string[] Products { get; set; }
        public string[] Currencies { get; set; }
        public string[] Countries { get; set; }
        public string[] Languages { get; set; }
        public string Remarks { get; set; }
    }
}