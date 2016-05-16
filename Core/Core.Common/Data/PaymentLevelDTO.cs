using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class PaymentLevelDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsDefault { get; set; }
        public string Code { get; set; }
    }
}