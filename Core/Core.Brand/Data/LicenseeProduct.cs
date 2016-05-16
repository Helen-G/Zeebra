using System;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class LicenseeProduct
    {
        public Guid LicenseeId { get; set; }
        public Licensee Licensee { get; set; }
        public Guid ProductId { get; set; }
    }
}