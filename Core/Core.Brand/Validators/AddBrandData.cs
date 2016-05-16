using System;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AddBrandData : IAddBrandData
    {
        public Guid Licensee { get; set; }
        public BrandType Type { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool EnablePlayerPrefix { get; set; }
        public string PlayerPrefix { get; set; }
        public PlayerActivationMethod PlayerActivationMethod { get; set; }
        public int InternalAccounts { get; set; }
        public string TimeZoneId { get; set; }
    }
}