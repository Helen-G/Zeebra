using System;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class BrandProduct
    {
        public Guid BrandId { get; set; }
        public Guid ProductId { get; set; }

        public Data.Brand Brand { get; set; }
    }
}