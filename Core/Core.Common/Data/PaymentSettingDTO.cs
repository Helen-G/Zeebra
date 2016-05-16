namespace AFT.RegoV2.Core.Common.Data
{
    public class PaymentSettingDTO
    {
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }

        public decimal MaxTransactionPerDay { get; set; }
        public decimal MaxTransactionPerWeek { get; set; }
        public decimal MaxTransactionPerMonth { get; set; }

        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
    }
}