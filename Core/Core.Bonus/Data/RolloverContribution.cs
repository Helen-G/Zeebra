namespace AFT.RegoV2.Core.Bonus.Data
{
    public class RolloverContribution: Identity
    {
        public virtual BonusTransaction Transaction { get; set; }
        public decimal Contribution { get; set; }
        public ContributionType Type { get; set; }
    }

    public enum ContributionType
    {
        Bet,
        Threshold,
        Cancellation
    }
}