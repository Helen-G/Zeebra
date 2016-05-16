using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Brand.Data;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class Licensee
    {
        public Licensee()
        {
            Brands = new List<Brand>();
            Currencies = new List<Currency>();
            Countries = new List<Country>();
            Cultures = new List<Culture>();
            Products = new List<LicenseeProduct>();
            Contracts = new List<Contract>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public bool AffiliateSystem { get; set; }
        public DateTimeOffset ContractStart { get; set; }
        public DateTimeOffset? ContractEnd { get; set; }
        public string Email { get; set; }
        public int AllowedBrandCount { get; set; }
        public int AllowedWebsiteCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }
        public string Remarks { get; set; }
        public string TimezoneId { get; set; }
        public LicenseeStatus Status { get; set; }

        public ICollection<Brand> Brands { get; set; }
        public ICollection<Currency> Currencies { get; set; }
        public ICollection<Country> Countries { get; set; }
        public ICollection<Culture> Cultures { get; set; }
        public ICollection<LicenseeProduct> Products { get; set; }
        public ICollection<Contract> Contracts { get; set; }

        public override bool Equals(object obj)
        {
            var licensee = obj as Licensee;
            return licensee != null && Id.Equals(licensee.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public enum LicenseeStatus
    {
        Inactive,
        Active,
        Deactivated
    }
}