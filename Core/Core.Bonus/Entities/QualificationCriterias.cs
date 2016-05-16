using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Core.Bonus.Entities
{
    internal class BaseQualificationCriteria
    {
        protected bool IsIssuedByCs { get; set; }
        private Bonus Bonus { get; set; }
        protected Data.Bonus Data { get { return Bonus.Data; } }
        protected DateTimeOffset Now { get { return SystemTime.Now.ToBrandOffset(Data.Template.Info.Brand.TimezoneId); } }

        public BaseQualificationCriteria(Bonus bonus, bool isIssuedByCs)
        {
            Bonus = bonus;
            IsIssuedByCs = isIssuedByCs;
        }

        public virtual IEnumerable<string> CheckForQualificationFailures(Player player, QualificationPhase phase)
        {
            if (Data.IsActive == false)
                yield return "Bonus is Deactivated";

            if (phase == QualificationPhase.Claim)
            {
                if (IsIssuedByCs == false)
                {
                    if (ClaimDurationIsOver())
                        yield return "Player cannot claim bonus outside of claim duration period.";
                }
            }
            else
            {
                if (IsPlayerInExcludeRiskLevel(player))
                    yield return "Player is in excluded risk level.";
                if (IsExcludeBonusesAlreadyRedeemed(player))
                    yield return "One of the disqualifying bonuses was already redeemed.";
                if (IsOverPlayerRedemptionLimit(player))
                    yield return "Bonus reached maximum redemption limit.";
            }

            if (phase == QualificationPhase.Redemption || phase == QualificationPhase.PreRedemption)
            {
                if (BonusIsNotActive(IsIssuedByCs))
                    yield return "Bonus is not active.";

                if (IsIssuedByCs == false)
                {
                    if (ApplyDurationIsOver())
                        yield return "Player cannot apply for bonus outside of application period.";
                }

                if (PlayersCurrencyIsNotQualified(player))
                    yield return "Currency is not qualified for this bonus.";
                if (IsParentBonusWasNotActivated(player))
                    yield return "Parent bonus was not activated.";

                if (IsOverRedemptionsLimit())
                    yield return "Bonus reached maximum player redemptions limit.";
                if (IsOverBonusRewardLimit(player))
                    yield return "Bonus reached maximum reward limit.";

                if (!PlayerHasAllowedVipLevel(player))
                    yield return "VIP level is not qualified for this bonus.";
                if (PlayerRegistrationDateDoesNotSatisfy(player.Data.DateRegistered))
                    yield return "Registration date is not qualified for this bonus.";
                if (PlayerWithinRegistrationDaysDoesNotSatisfy(player.Data.DateRegistered))
                    yield return "Bonus is qualified within X registration days.";
            }
        }

        bool IsPlayerInExcludeRiskLevel(Player player)
        {
            var excludeRiskLevels = Data.Template.Availability.ExcludeRiskLevels;
            if (excludeRiskLevels.Count == 0)
                return false;

            var excludeIds = excludeRiskLevels.Select(ex => ex.ExcludedRiskLevelId);
            var riskLevelIds = player.Data.RiskLevels.Where(rl => rl.IsActive).Select(rl => rl.Id);
            return riskLevelIds.Intersect(excludeIds).Any();
        }
        bool PlayersCurrencyIsNotQualified(Player player)
        {
            return Data.Template.Rules.RewardTiers.Any(t => t.CurrencyCode == player.Data.CurrencyCode) == false;
        }
        bool PlayerRegistrationDateDoesNotSatisfy(DateTimeOffset dateRegistered)
        {
            var withoutRegistrationDateRange = Data.Template.Availability.PlayerRegistrationDateFrom.HasValue == false &&
                Data.Template.Availability.PlayerRegistrationDateTo.HasValue == false;
            if (withoutRegistrationDateRange)
                return false;

            if (Data.Template.Availability.PlayerRegistrationDateTo.HasValue == false)
            {
                return dateRegistered < Data.Template.Availability.PlayerRegistrationDateFrom;
            }

            return dateRegistered < Data.Template.Availability.PlayerRegistrationDateFrom || dateRegistered > Data.Template.Availability.PlayerRegistrationDateTo;
        }
        bool PlayerWithinRegistrationDaysDoesNotSatisfy(DateTimeOffset dateRegistered)
        {
            if (Data.Template.Availability.WithinRegistrationDays == 0)
                return false;
            return Now.Date > dateRegistered.AddDays(Data.Template.Availability.WithinRegistrationDays).Date;
        }
        bool PlayerHasAllowedVipLevel(Player player)
        {
            if (Data.Template.Availability.VipLevels.Any() == false)
                return true;
            return Data.Template.Availability.VipLevels.Any(a => a.Code == player.Data.VipLevel);
        }
        bool IsOverBonusRewardLimit(Player player)
        {
            var rewardTier = Data.Template.Rules.RewardTiers.SingleOrDefault(rt => rt.CurrencyCode == player.Data.CurrencyCode);
            if (rewardTier == null)
                return false;
            var limit = rewardTier.RewardAmountLimit;
            if (limit == 0)
                return false;

            return Data.Statistic.TotalRedeemedAmount >= limit;
        }
        bool IsOverRedemptionsLimit()
        {
            var limit = Data.Template.Availability.RedemptionsLimit;
            if (limit == 0)
                return false;

            return Data.Statistic.TotalRedemptionCount >= limit;
        }
        bool IsParentBonusWasNotActivated(Player player)
        {
            var parentBonusId = Data.Template.Availability.ParentBonusId;
            if (parentBonusId.HasValue == false)
                return false;

            return player.BonusesRedeemed.Any(bonus => bonus.Bonus.Id == parentBonusId && bonus.ActivationState == ActivationStatus.Activated) == false;
        }
        /// <summary>
        /// Disqualifies player if he redeemed (ANY|ALL) bonus(es) from exclude list
        /// </summary>
        bool IsExcludeBonusesAlreadyRedeemed(Player player)
        {
            var excludeBonuses = Data.Template.Availability.ExcludeBonuses;
            if (excludeBonuses.Count == 0)
                return false;

            var bonusRedemptions = player.BonusesRedeemed.Where(bonus =>
                bonus.ActivationState != ActivationStatus.Negated &&
                bonus.ActivationState != ActivationStatus.Canceled);

            var excludeIds = excludeBonuses.Select(ex => ex.ExcludedBonusId);
            return Data.Template.Availability.ExcludeOperation == Operation.All ?
                excludeBonuses.All(exc => bonusRedemptions.Select(br => br.Bonus.Id).Contains(exc.ExcludedBonusId)) :
                bonusRedemptions.Any(br => excludeIds.Contains(br.Bonus.Id));
        }
        bool BonusIsNotActive(bool isIssuedByCs)
        {
            if (isIssuedByCs)
            {
                return Now.Date < Data.ActiveFrom.Date;
            }

            return Data.ActiveFrom.Date > Now.Date || Data.ActiveTo.Date < Now.Date;
        }
        bool IsOverPlayerRedemptionLimit(Player player)
        {
            var limit = Data.Template.Availability.PlayerRedemptionsLimit;
            if (limit == 0)
                return false;

            if (Data.Template.Availability.PlayerRedemptionsLimitType == BonusPlayerRedemptionsLimitType.Lifetime)
                return player.BonusesRedeemed
                    .Count(r => r.Bonus.Id == Data.Id && r.ActivationState == ActivationStatus.Activated) >= limit;

            DateTimeOffset from = Now;
            from = from.Subtract(from.TimeOfDay).ToBrandOffset(Data.Template.Info.Brand.TimezoneId);
            DateTimeOffset to = from;

            if (Data.Template.Availability.PlayerRedemptionsLimitType == BonusPlayerRedemptionsLimitType.Day)
            {
                to = from.AddDays(1);
            }

            if (Data.Template.Availability.PlayerRedemptionsLimitType == BonusPlayerRedemptionsLimitType.Week)
            {
                int dayOfWeekOffset = from.DayOfWeek - DayOfWeek.Monday;
                if (dayOfWeekOffset < 0)
                    dayOfWeekOffset += 7;

                from = from.AddDays(-1 * dayOfWeekOffset);
                to = from.AddDays(7);
            }

            if (Data.Template.Availability.PlayerRedemptionsLimitType == BonusPlayerRedemptionsLimitType.Month)
            {
                from = from.AddDays(1 - from.Date.Day);
                to = from.AddMonths(1);
            }

            int redeemed =
                player.BonusesRedeemed.Count(
                    r =>
                        r.Bonus.Id == Data.Id && r.ActivationState == ActivationStatus.Activated &&
                        r.UpdatedOn.HasValue && r.UpdatedOn.Value >= from && r.UpdatedOn.Value <= to);
            return redeemed >= limit;
        }

        bool ClaimDurationIsOver()
        {
            var daysToClaim = Data.DaysToClaim > 0 ? Data.DaysToClaim : 99999;
            return Data.ActiveFrom.Date > Now.Date || Data.ActiveTo.AddDays(daysToClaim).Date < Now.Date;
        }
        bool ApplyDurationIsOver()
        {
            if (Data.DurationType == DurationType.None)
                return false;

            return Now < Data.DurationStart || Now >= Data.DurationEnd;
        }

        protected bool BonusTiersDoNotSatisfy(string currencyCode, decimal affectedAmount)
        {
            var rewardTier = Data.Template.Rules.RewardTiers.SingleOrDefault(rt => rt.CurrencyCode == currencyCode);
            if (rewardTier == null)
                return false;
            return Bonus.GetMatchingBonusTierOrNull(rewardTier, affectedAmount) == null;
        }
    }

    internal class DepositQualificationCriteria : BaseQualificationCriteria
    {
        public DepositQualificationCriteria(Bonus bonus, bool isIssuedByCs) : base(bonus, isIssuedByCs) { }

        public IEnumerable<string> CheckForQualificationFailures(Player player, QualificationPhase phase, decimal depositAmount)
        {
            foreach (var qualificationFailure in CheckForQualificationFailures(player, phase))
            {
                yield return qualificationFailure;
            }

            if (phase == QualificationPhase.PreRedemption || phase == QualificationPhase.Redemption)
            {
                if (Data.Template.Info.DepositKind == DepositKind.First)
                {
                    if (IsIssuedByCs == false)
                    {
                        if (player.Data.DepositTransactionsCount != 0)
                            yield return "Bonus can be redeemed during first deposit only.";
                    }
                }
                else if (Data.Template.Info.DepositKind == DepositKind.Reload)
                {
                    if (IsIssuedByCs == false)
                    {
                        if (player.Data.DepositTransactionsCount < 1)
                            yield return "Bonus cannot be redeemed during first deposit.";
                    }
                }
                else if (Data.Template.Info.DepositKind == DepositKind.High)
                {
                    var highDepositAmount = player.MonthlyAccumulatedDepositAmount;
                    if (HighDepositTiersDoNotSatisfy(player.Data.CurrencyCode, highDepositAmount))
                    {
                        yield return "High Deposit Tier not found.";
                    }
                    else if (PlayerRedeemedAllHighDepositTiers(player, highDepositAmount))
                        yield return "All high deposit rewards are redeemed this month.";
                }
            }

            if (Data.Template.Info.DepositKind == DepositKind.First || Data.Template.Info.DepositKind == DepositKind.Reload)
            {
                if (phase == QualificationPhase.Redemption || phase == QualificationPhase.Activation)
                {
                    if (BonusTiersDoNotSatisfy(player.Data.CurrencyCode, depositAmount))
                        yield return "Deposit amount is not qualified for this bonus.";
                }
            }
        }

        bool HighDepositTiersDoNotSatisfy(string currencyCode, decimal monthlyDepositAmount)
        {
            var rewardTier = Data.Template.Rules.RewardTiers.SingleOrDefault(rt => rt.CurrencyCode == currencyCode);
            if (rewardTier == null)
                return false;

            return rewardTier.HighDepositTiers.Count(r => monthlyDepositAmount >= r.From) == 0;
        }

        bool PlayerRedeemedAllHighDepositTiers(Player player, decimal monthlyDepositAmount)
        {
            var rewardTier = Data.Template.Rules.RewardTiers.Single(rt => rt.CurrencyCode == player.Data.CurrencyCode);
            var rewardsReceived = player.BonusesRedeemed.Count(br =>
                br.Bonus.Id == Data.Id &&
                (br.ActivationState == ActivationStatus.Activated || (br.ActivationState == ActivationStatus.Pending && br.RolloverState == RolloverStatus.Active)) &&
                br.CreatedOn.Month == Now.Month &&
                br.CreatedOn.Year == Now.Year);

            if (Data.Template.Rules.IsAutoGenerateHighDeposit)
            {
                var tiersCount = Math.Floor(monthlyDepositAmount / rewardTier.BonusTiers.Single().From);
                return rewardsReceived == tiersCount;
            }
            else
            {
                var tiersCount = rewardTier.BonusTiers.Count(bt => monthlyDepositAmount >= bt.From);
                return tiersCount == rewardsReceived;
            }
        }
    }

    internal class MobileEmailVerificationQualificationCriteria : BaseQualificationCriteria
    {
        public MobileEmailVerificationQualificationCriteria(Bonus bonus, bool isIssuedByCs) : base(bonus, isIssuedByCs) { }

        public override IEnumerable<string> CheckForQualificationFailures(Player player, QualificationPhase phase)
        {
            foreach (var qualificationFailure in base.CheckForQualificationFailures(player, phase))
            {
                yield return qualificationFailure;
            }

            if (phase == QualificationPhase.Activation)
            {
                if (MobileNumberIsntVerified(player))
                    yield return "Mobile number is not verified.";
                if (EmailIsntVerified(player))
                    yield return "Email address is not verified.";
            }
            else if (phase == QualificationPhase.PreRedemption || phase == QualificationPhase.Redemption)
            {
                if (IsMobilePlusEmailVerificationBonusRedemptionExist(player))
                    yield return "One verification bonus redemption per player is allowed.";
            }
        }

        bool IsMobilePlusEmailVerificationBonusRedemptionExist(Player player)
        {
            return player.BonusesRedeemed.Any(redemption => redemption.Bonus.Template.Info.BonusTrigger == Trigger.MobilePlusEmailVerification);
        }

        bool EmailIsntVerified(Player player)
        {
            return player.Data.IsEmailVerified == false;
        }

        bool MobileNumberIsntVerified(Player player)
        {
            return player.Data.IsMobileVerified == false;
        }
    }

    internal class FundInQualificationCriteria : BaseQualificationCriteria
    {
        public FundInQualificationCriteria(Bonus bonus, bool isIssuedByCs) : base(bonus, isIssuedByCs) { }

        public IEnumerable<string> CheckForQualificationFailures(Player player, QualificationPhase phase, RedemptionParams redemptionParams)
        {
            foreach (var qualificationFailure in base.CheckForQualificationFailures(player, phase))
            {
                yield return qualificationFailure;
            }

            if ((phase == QualificationPhase.PreRedemption && IsIssuedByCs == false) || phase == QualificationPhase.Redemption)
            {
                if (FundInWalletIsNotQualified(redemptionParams.TransferWalletTemplateId.Value))
                    yield return "Fund in wallet is not qualified for this bonus.";
            }

            if (phase == QualificationPhase.Redemption || phase == QualificationPhase.Activation)
            {
                if (BonusTiersDoNotSatisfy(player.Data.CurrencyCode, redemptionParams.TransferAmount))
                    yield return "Fund in amount is not qualified for this bonus.";
            }
        }

        bool FundInWalletIsNotQualified(Guid walletId)
        {
            return Data.Template.Rules.FundInWallets.Any(pr => pr.WalletId == walletId) == false;
        }
    }
}