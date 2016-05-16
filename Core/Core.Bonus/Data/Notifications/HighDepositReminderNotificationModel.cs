namespace AFT.RegoV2.Core.Bonus.Data.Notifications
{
    public class HighDepositReminderNotificationModel
    {
        public string Username { get; set; }
        public string Currency { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal DepositAmountRequired { get; set; }
        public string Brand { get; set; }
    }
}