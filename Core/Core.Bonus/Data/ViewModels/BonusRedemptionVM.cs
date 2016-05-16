namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class BonusRedemptionVM
    {
        public string LicenseeName { get; set; }
        public string BrandName { get; set; }
        public string Username { get; set; }
        public string BonusName { get; set; }
        public int ActivationState { get; set; }
        public int RolloverState { get; set; }
        public decimal Amount { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal Rollover { get; set; }
    }
}