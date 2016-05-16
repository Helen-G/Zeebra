using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class DeactivateBrandData : IDeactivateBrandData
    {
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }
    }
}