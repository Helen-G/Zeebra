using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditLicenseeModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public bool AffiliateSystem { get; set; }
        public string ContractStart { get; set; }
        public string ContractEnd { get; set; }
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