using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class PlayerTransactionRecord
    {
        [Key]
        public Guid TransactionId { get; set; }

        [Index]
        public DateTimeOffset CreatedOn { get; set; }

        [Index]
        public Guid PlayerId { get; set; }
        
        [Index, MaxLength(100)]
        public string PerformedBy { get; set; }

        [Index, MaxLength(100)]
        public string Wallet { get; set; }
        
        [Index]
        public Guid? RoundId { get; set; }
        [Index]
        public Guid? GameId { get; set; }
        [Index, MaxLength(100)]
        public string Type { get; set; }
        [Index]
        public decimal MainBalanceAmount { get; set; }
        [Index]
        public decimal BonusBalanceAmount { get; set; }
        [Index]
        public decimal TemporaryBalanceAmount { get; set; }
        [Index]
        public decimal LockBonusAmount { get; set; }
        [Index]
        public decimal LockFraudAmount { get; set; }
        [Index]
        public decimal LockWithdrawalAmount { get; set; }
        [Index]
        public decimal MainBalance { get; set; }
        [Index]
        public decimal BonusBalance { get; set; }
        [Index]
        public decimal TemporaryBalance { get; set; }
        [Index]
        public decimal LockBonus { get; set; }
        [Index]
        public decimal LockFraud { get; set; }
        [Index]
        public decimal LockWithdrawal { get; set; }

        [Index, DefaultValue(false)]
        public bool IsInternal { get; set; }

        [Index, MaxLength(100)]
        public string CurrencyCode { get; set; }

        [Index, MaxLength(200)]
        public string Description { get; set; }

        [Index, MaxLength(100)]
        public string TransactionNumber { get; set; }
        
        [Index]
        public Guid? RelatedTransactionId { get; set; }
    }

    public enum ReportTransactionType
    {
        Deposit,
        Withdraw,
        Bonus,
        BonusCancelled,
        BetPlaced,
        BetWon,
        BetLost,
        BetCancelled,
        WageringFinished,
        BetWonAdjustment,
        FundIn,
        FundOut,
        LockWithdrawal,
        LockBonus,
        LockFraud,
        UnlockWithdrawal,
        UnlockBonus,
        UnlockFraud
    }
}
