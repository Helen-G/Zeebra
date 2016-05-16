using System;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class Brand
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid LicenseeId { get; set; }
        public Licensee Licensee { get; set; }
        public string LicenseeName { get; set; }
        public string BaseCurrencyCode { get; set; }
    }
}