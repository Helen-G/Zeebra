using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data.Bonus;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Bonus.Entities
{
    public class Bonus
    {
        private readonly Data.Bonus _data;
        internal Data.Bonus Data { get { return _data; } }

        public Bonus(Data.Bonus data)
        {
            _data = data;
        }

        public IEnumerable<string> QualifyFor(Player player, QualificationPhase phase, RedemptionParams redemptionParams)
        {
            switch (_data.Template.Info.BonusTrigger)
            {
                case Trigger.Deposit:
                    return new DepositQualificationCriteria(this, redemptionParams.IsIssuedByCs).CheckForQualificationFailures(player, phase, redemptionParams.TransferAmount);
                case Trigger.MobilePlusEmailVerification:
                    return new MobileEmailVerificationQualificationCriteria(this, redemptionParams.IsIssuedByCs).CheckForQualificationFailures(player, phase);
                case Trigger.FundIn:
                    return new FundInQualificationCriteria(this, redemptionParams.IsIssuedByCs).CheckForQualificationFailures(player, phase, redemptionParams);
                default:
                    return new BaseQualificationCriteria(this, redemptionParams.IsIssuedByCs).CheckForQualificationFailures(player, phase);
            }
        }
        public bool QualifiesFor(Player player, QualificationPhase phase, RedemptionParams redemptionParams)
        {
            return QualifyFor(player, phase, redemptionParams).Any() == false;
        }
        public decimal CalculateReward(Player player, RedemptionParams redemptionParams)
        {
            var reward = CalculateBonusAmount(player, redemptionParams);
            var cappedBonus = LimitRewardByBonusRewardAmountLimit(reward, player);

            return cappedBonus;
        }
        public BonusRewardThreshold CalculateRewardThreshold(Player player)
        {
            var monthlyAccumulatedDepositAmount = player.MonthlyAccumulatedDepositAmount;

            var sortedTiers = GetHighDepositTiers(player.Data.CurrencyCode, monthlyAccumulatedDepositAmount);
            var affectedTiers = sortedTiers
                .Where(t => monthlyAccumulatedDepositAmount >= t.From * t.NotificationPercentThreshold)
                .ToList();

            if (affectedTiers.Any())
            {
                var maxDepositAmount = affectedTiers.Max(t => t.From);
                var tier = affectedTiers.Single(t => t.From == maxDepositAmount);
                var remainingAmount = tier.From - monthlyAccumulatedDepositAmount;

                if (remainingAmount > 0)
                {
                    return new BonusRewardThreshold
                    {
                        BonusAmount = tier.Reward,
                        DepositAmountRequired = tier.From,
                        RemainingAmount = remainingAmount
                    };
                }
            }

            return null;
        }

        public TierBase GetMatchingBonusTierOrNull(RewardTier rewardTier, decimal affectedValue)
        {
            var tierList = rewardTier.BonusTiers.OrderBy(rt => rt.DateCreated).ToList();
            for (var i = 0; i < tierList.Count; i++)
            {
                var current = tierList[i];
                var to = decimal.MaxValue;
                if (i < rewardTier.BonusTiers.Count - 1)
                    to = tierList[i + 1].From;

                if ((affectedValue >= current.From) && (affectedValue < to))
                {
                    return current;
                }
            }
            return null;
        }

        private decimal CalculateBonusAmount(Player player, RedemptionParams redemptionParams)
        {
            if (Data.Template.Info.BonusTrigger == Trigger.Deposit && Data.Template.Info.DepositKind == DepositKind.High)
            {
                return CalculateHighDepositBonusReward(player);
            }

            var affectedValue = GetTierAffectedAmount(player, redemptionParams);

            var reward = 0m;
            var matchingRewardTier = Data.Template.Rules.RewardTiers.Single(t => t.CurrencyCode == player.Data.CurrencyCode);

            var matchingTier = GetMatchingBonusTierOrNull(matchingRewardTier, affectedValue) as BonusTier;

            if (matchingTier != null)
            {
                if (Data.Template.Rules.RewardType == BonusRewardType.TieredAmount || Data.Template.Rules.RewardType == BonusRewardType.Amount)
                {
                    reward = matchingTier.Reward;
                }
                else
                {
                    reward = affectedValue * matchingTier.Reward;
                    reward = LimitRewardByTransactionLimit(reward, matchingTier.MaxAmount);
                }
            }

            return reward;
        }
        private decimal GetTierAffectedAmount(Player player, RedemptionParams redemptionParams)
        {
            switch (Data.Template.Info.BonusTrigger)
            {
                case Trigger.Deposit:
                    return redemptionParams.TransferAmount;
                case Trigger.FundIn:
                    return redemptionParams.TransferAmount;
                case Trigger.ReferFriend:
                    var referralCount = player.BonusesRedeemed.Count(r =>
                        r.Bonus.Id == Data.Id &&
                        r.Bonus.Template.Info.BonusTrigger == Trigger.ReferFriend &&
                        r.ActivationState == ActivationStatus.Activated);
                    return referralCount + 1;
                case Trigger.MobilePlusEmailVerification:
                    return decimal.Zero;
                default:
                    throw new RegoException("Bonus type is not supported.");
            }
        }
        private decimal CalculateHighDepositBonusReward(Player player)
        {
            var sortedTiers = GetHighDepositTiers(player.Data.CurrencyCode, player.MonthlyAccumulatedDepositAmount);

            var now = DateTimeOffset.Now.ToBrandOffset(player.Data.Brand.TimezoneId);
            var currentTierIndex = player.BonusesRedeemed.Count(br =>
                br.Bonus.Id == Data.Id &&
                br.ActivationState == ActivationStatus.Activated &&
                br.CreatedOn.Month == now.Month &&
                br.CreatedOn.Year == now.Year);
            var currentTier = sortedTiers.ElementAt(currentTierIndex);

            return currentTier.Reward;
        }
        private List<HighDepositTier> GetHighDepositTiers(string currencyCode, decimal transactionAmount)
        {
            var tiers = new List<HighDepositTier>();
            var rewardTier = Data.Template.Rules.RewardTiers.SingleOrDefault(t => t.CurrencyCode == currencyCode);
            if (rewardTier != null)
            {
                if (Data.Template.Rules.IsAutoGenerateHighDeposit)
                {
                    var templateTier = rewardTier.HighDepositTiers.Single();
                    var tiersCount = Math.Ceiling(transactionAmount / templateTier.From);
                    for (var i = 1; i <= tiersCount; i++)
                    {
                        tiers.Add(new HighDepositTier
                        {
                            Reward = templateTier.Reward,
                            From = templateTier.From * i
                        });
                    }
                }
                else
                {
                    tiers = rewardTier.HighDepositTiers;
                }
            }
            return tiers;
        }

        private decimal LimitRewardByTransactionLimit(decimal reward, decimal transactionLimit)
        {
            return transactionLimit == decimal.Zero ? reward : Math.Min(reward, transactionLimit);
        }
        private decimal LimitRewardByBonusRewardAmountLimit(decimal reward, Player player)
        {
            var limit = Data.Template.Rules.RewardTiers.Single(rt => rt.CurrencyCode == player.Data.CurrencyCode).RewardAmountLimit;
            if (limit == decimal.Zero) return reward;

            var amountCanBeCredited = limit - Data.Statistic.TotalRedeemedAmount;
            return Math.Min(reward, amountCanBeCredited);
        }

        public IEnumerable<QualifiedTransaction> GetQualifiedTransactions(Player player)
        {
            var transactions = new List<BonusTransaction>();
            var bonusTrigger = Data.Template.Info.BonusTrigger;
            var depositKind = Data.Template.Info.DepositKind;
            if (bonusTrigger == Trigger.Deposit && depositKind != DepositKind.High)
            {
                var depositTransactions = player.Data.Wallets
                    .SelectMany(w => w.Transactions)
                    .Where(tt => tt.Type == TransactionType.Deposit && tt.CreatedOn >= Data.ActiveFrom && tt.CreatedOn < Data.ActiveTo)
                    .OrderBy(t => t.CreatedOn);
                if (depositTransactions.Any())
                {
                    transactions = depositKind == DepositKind.First ? depositTransactions.Take(1).ToList() : depositTransactions.Skip(1).ToList();
                }
            }
            else if (bonusTrigger == Trigger.FundIn)
            {
                var qualifiedWallets = Data.Template.Rules.FundInWallets.Select(w => w.WalletId);
                transactions = player.Data.Wallets
                    .Where(w => qualifiedWallets.Contains(w.TemplateId))
                    .SelectMany(w => w.Transactions)
                    .Where(tt => tt.Type == TransactionType.FundIn && tt.CreatedOn >= Data.ActiveFrom && tt.CreatedOn < Data.ActiveTo)
                    .ToList();
            }
            else
            {
                throw new RegoException("Bonus type is not supported.");
            }

            foreach (var transaction in transactions)
            {
                var reward = CalculateBonusAmount(player, new RedemptionParams { TransferAmount = transaction.TotalAmount });
                if (reward > 0m)
                {
                    yield return new QualifiedTransaction
                    {
                        CurrencyCode = player.Data.CurrencyCode,
                        BonusAmount = reward,
                        Amount = transaction.TotalAmount,
                        Date = transaction.CreatedOn,
                        Id = transaction.Id
                    };
                }
            }
        }
    }

    public enum QualificationPhase
    {
        PreRedemption,
        Redemption,
        Activation,
        Claim
    }

    public class BonusRewardThreshold
    {
        public decimal BonusAmount { get; set; }
        public decimal DepositAmountRequired { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}