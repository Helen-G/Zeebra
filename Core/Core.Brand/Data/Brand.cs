using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data.Brand;
using Newtonsoft.Json;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class Brand
    {
        public Brand()
        {
            BrandCultures = new List<BrandCulture>();
            BrandCurrencies = new List<BrandCurrency>();
            BrandCountries = new List<BrandCountry>();
            VipLevels = new List<VipLevel>();
            Products = new List<BrandProduct>();
            WalletTemplates = new List<WalletTemplate>();
        }

        public Guid             Id { get; set; }
        public Guid             LicenseeId { get; set; }
        public Guid?            DefaultVipLevelId { get; set; }
        public string           DefaultCulture { get; set; }
        public string           DefaultCurrency { get; set; }
        public string           BaseCurrency { get; set; }

        
        public string           Code { get; set; }
        public string           Name { get; set; }
        public BrandType        Type { get; set; }
        public string           TimezoneId { get; set; }
        public bool             EnablePlayerPrefix { get; set; }
        public string           PlayerPrefix { get; set; }
        public BrandStatus      Status { get; set; }



        public string           CreatedBy { get; set; }
        public DateTimeOffset   DateCreated { get; set; }
        public string           UpdatedBy { get; set; }
        public DateTimeOffset?  DateUpdated { get; set; }
        public string           ActivatedBy { get; set; }
        public DateTimeOffset?  DateActivated { get; set; }
        public string           DeactivatedBy { get; set; }
        public DateTimeOffset?  DateDeactivated { get; set; }

        public string           Remarks { get; set; }
        public int              InternalAccountsNumber { get; set; }
        public PlayerActivationMethod PlayerActivationMethod { get; set; }
        public string           EmailActivationUrl { get; set; }

        public DateTime?    CurrencySetCreated { get; set; }
        public string       CurrencySetCreatedBy { get; set; }
        public DateTime?    CurrencySetUpdated { get; set; }
        public string       CurrencySetUpdatedBy { get; set; }

        [JsonIgnore]
        public Licensee     Licensee { get; set; }
        public VipLevel     DefaultVipLevel { get; set; }

        public ICollection<BrandCulture>    BrandCultures { get; set; }
        public ICollection<BrandCurrency>   BrandCurrencies { get; set; }
        public ICollection<BrandCountry>    BrandCountries { get; set; }
        public ICollection<VipLevel>        VipLevels { get; set; }
        public ICollection<WalletTemplate>  WalletTemplates { get; set; }
        public ICollection<BrandProduct> Products { get; set; }
        


        public override bool Equals(object obj)
        {
            var brand = obj as Brand;
            return brand != null && Id.Equals(brand.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class BrandId
    {
        private readonly Guid _id;

        public BrandId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(BrandId id)
        {
            return id._id;
        }

        public static implicit operator BrandId(Guid id)
        {
            return new BrandId(id);
        }
    }


}