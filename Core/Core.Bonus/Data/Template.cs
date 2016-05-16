using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class Template : Identity
    {
        public int Version { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }
        public TemplateStatus Status { get; set; }

        public virtual TemplateInfo Info { get; set; }
        public virtual TemplateAvailability Availability { get; set; }
        public virtual TemplateRules Rules { get; set; }
        public virtual TemplateWagering Wagering { get; set; }
        public virtual TemplateNotification Notification { get; set; }
    }

    public class TemplateInfo: Identity
    {
        public virtual Brand Brand { get; set; }
        public string Name { get; set; }
        public Trigger BonusTrigger { get; set; }
        public DepositKind? DepositKind { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Determines where to credit bonus funds and issue rollover to
        /// </summary>
        public Guid WalletTemplateId { get; set; }
        public IssuanceMode Mode { get; set; }
        /// <summary>
        /// Determines if bonus reward can be withdrawed from bonus balance
        /// </summary>
        public bool IsWithdrawable { get; set; }
    }

    public class TemplateAvailability : Identity
    {
        public TemplateAvailability()
        {
            VipLevels = new List<BonusVip>();
            ExcludeBonuses = new List<BonusExclude>();
            ExcludeRiskLevels = new List<RiskLevelExclude>();
        }

        /// <summary>
        /// Bonus id that player should redeem first in order to be qualified for THIS bonus
        /// </summary>
        public Guid? ParentBonusId { get; set; }
        public DateTimeOffset? PlayerRegistrationDateFrom { get; set; }
        public DateTimeOffset? PlayerRegistrationDateTo { get; set; }
        /// <summary>
        /// Specifies number of days after player registration bonus is qualified
        /// </summary>
        public int WithinRegistrationDays { get; set; }
        public virtual List<BonusVip> VipLevels { get; set; }
        public Operation ExcludeOperation { get; set; }
        public virtual List<BonusExclude> ExcludeBonuses { get; set; }
        public virtual List<RiskLevelExclude> ExcludeRiskLevels { get; set; } 

        /// <summary>
        /// Limits number of redemptions per player
        /// </summary>
        public int PlayerRedemptionsLimit { get; set; }
        public BonusPlayerRedemptionsLimitType PlayerRedemptionsLimitType { get; set; }
        /// <summary>
        /// Limits number of bonus redemptions
        /// </summary>
        public int RedemptionsLimit { get; set; }
    }

    public enum Operation
    {
        Any,
        All
    }

    public class TemplateRules : Identity
    {
        public TemplateRules()
        {
            FundInWallets = new List<BonusFundInWallet>();
        }

        public BonusRewardType RewardType { get; set; }
        public virtual List<RewardTier> RewardTiers { get; set; }

        public bool IsAutoGenerateHighDeposit { get; set; }

        public decimal ReferFriendMinDepositAmount { get; set; }
        public decimal ReferFriendWageringCondition { get; set; }

        public virtual List<BonusFundInWallet> FundInWallets { get; set; }
    }

    public class TemplateWagering : Identity
    {
        public TemplateWagering()
        {
            GameContributions = new List<GameContribution>();
        }

        /// <summary>
        /// Determines if rollover affected amount should be locked for withdrawal until rollover is satisfied
        /// </summary>
        public bool HasWagering { get; set; }
        /// <summary>
        /// Determines wagering amount value for wagering calculation
        /// </summary>
        public WageringMethod Method { get; set; }
        /// <summary>
        /// Determines multiplier for wagering calculation
        /// </summary>
        public decimal Multiplier { get; set; }
        /// <summary>
        /// Determines Total balance value, reaching which will zero out rollover
        /// </summary>
        public decimal Threshold { get; set; }
        /// <summary>
        /// Specifies contribution of games to wagering requirement fulfillment
        /// </summary>
        public virtual List<GameContribution> GameContributions { get; set; }

        /// <summary>
        /// Issue bonus reward after wagering requirement is fulfilled or issue it immediately
        /// </summary>
        public bool IsAfterWager { get; set; }
    }

    public class TemplateNotification : Identity
    {
        public Guid? EmailTemplateId { get; set; }
        public Guid? SmsTemplateId { get; set; }
    }

    public enum Trigger
    {
        Deposit,
        ReferFriend,
        MobilePlusEmailVerification,
        FundIn
    }

    public enum DepositKind
    {
        Reload,
        First,
        High
    }

    public enum BonusRewardType
    {
        Amount,
        Percentage,
        TieredAmount,
        TieredPercentage
    }

    public enum BonusPlayerRedemptionsLimitType
    {
        Lifetime,
        Month,
        Week,
        Day
    }

    public enum WageringMethod
    {
        Bonus,
        TransferAmount,
        BonusAndTransferAmount
    }

    public enum TemplateStatus
    {
        Incomplete,
        Complete,
        Deleted
    }

    public class GameContribution: Identity
    {
        public Guid GameId { get; set; }
        public decimal Contribution { get; set; }
    }

    public class BonusFundInWallet: Identity
    {
        public Guid WalletId { get; set; }
    }

    public class BonusVip: Identity
    {
        public string Code { get; set; }
    }

    public class BonusExclude: Identity
    {
        public Guid ExcludedBonusId { get; set; }
    }

    public class RiskLevelExclude : Identity
    {
        public Guid ExcludedRiskLevelId { get; set; }
    }

    public class TierBase: Identity
    {
        public DateTime DateCreated { get; set; }
        public decimal From { get; set; }
        public decimal Reward { get; set; }
    }

    public class BonusTier : TierBase
    {
        public decimal? To { get; set; }
        public decimal MaxAmount { get; set; }
    }

    public class HighDepositTier : TierBase
    {
        public decimal NotificationPercentThreshold { get; set; }
    }
}