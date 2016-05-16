using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class ActivateBrandData : IActivateBrandData
    {
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }
    }
}