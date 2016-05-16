using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public enum BalanceTarget
    {
        Main,
        Bonus
    }

    public class IssuanceParams
    {
        public Guid WalletTemplateId { get; set; }
        public decimal Amount { get; set; }
        public BalanceTarget Target { get; set; }
    }

    public class LockUnlockParams
    {
        public Guid? WalletTemplateId { get; set; }
        public decimal Amount { get; set; }
        public LockType Type { get; set; }
        public string Description { get; set; }
    }

    public enum LockType
    {
        Withdrawal,
        Bonus,
        Fraud
    }

    public class AdjustmentParams
    {
        public AdjustmentParams(AdjustmentReason reason)
        {
            Reason = reason;
        }

        public Guid WalletTemplateId { get; set; }
        public AdjustmentReason Reason { get; set; }
        public decimal TemporaryBalanceAdjustment { get; set; }
        public decimal MainBalanceAdjustment { get; set; }
        public decimal BonusBalanceAdjustment { get; set; }
        public Guid? RelatedTransactionId { get; set; }
    }

    public enum AdjustmentReason
    {
        BonusCancelled,
        WageringFinished
    }

    public class BetWonAdjustmentParams
    {
        public Guid WalletTemplateId { get; set; }
        public bool BetWonDuringRollover { get; set; }
        public Guid RelatedTransactionId { get; set; }
    }
}