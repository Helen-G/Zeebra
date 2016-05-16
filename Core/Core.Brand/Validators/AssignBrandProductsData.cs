using System;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandProductsData
    {
        public Guid BrandId { get; set; }
        public Guid[] ProductsIds { get; set; }
    }
}