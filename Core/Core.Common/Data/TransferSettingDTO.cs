namespace AFT.RegoV2.Core.Shared.Data
{
    public class TransferSettingDTO
    {
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }

        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }

        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
    }
}