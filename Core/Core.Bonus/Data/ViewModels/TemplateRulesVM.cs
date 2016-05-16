using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Bonus.Data.ViewModels
{
    public class TemplateRulesVM
    {
        public TemplateRulesVM()
        {
            RewardTiers = new List<RewardTierVM>();
            FundInWallets = new List<Guid>();
        }

        public BonusRewardType RewardType { get; set; }
        public List<RewardTierVM> RewardTiers { get; set; }
        public bool IsAutoGenerateHighDeposit { get; set; }

        public decimal ReferFriendMinDepositAmount { get; set; }
        public decimal ReferFriendWageringCondition { get; set; }

        public List<Guid> FundInWallets { get; set; }
    }

    public class RewardTierVM
    {
        public RewardTierVM()
        {
            BonusTiers = new List<TemplateTierVM>();
        }

        public string CurrencyCode { get; set; }
        public List<TemplateTierVM> BonusTiers { get; set; }
        public decimal RewardAmountLimit { get; set; }
    }

    public class TemplateTierVM
    {
        public decimal Reward { get; set; }
        public decimal From { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal? To { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal NotificationPercentThreshold { get; set; }
    }
}