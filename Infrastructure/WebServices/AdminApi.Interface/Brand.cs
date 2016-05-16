using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminApi.Interface
{
    //public class AddBrandRequest
    //{
    //    public Guid Licensee { get; set; }
    //    public BrandType Type { get; set; }
    //    public string Name { get; set; }
    //    public string Code { get; set; }
    //    public bool EnablePlayerPrefix { get; set; }
    //    public string PlayerPrefix { get; set; }
    //    public PlayerActivationMethod PlayerActivationMethod { get; set; }
    //    public int InternalAccounts { get; set; }
    //    public string TimeZoneId { get; set; }
    //}

    public class AddBrandResponse
    {
        public Guid BrandId { get; set; }
    }

    public class BrandsResponse
    {
        public List<Brand> Brands { get; set; }
    }

    public class Brand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LicenseeId { get; set; }
        public IEnumerable<Currency> Currencies { get; set; }
        public IEnumerable<VipLevel> VipLevels { get; set; }
        public bool IsSelectedInFilter { get; set; }
    }
}
