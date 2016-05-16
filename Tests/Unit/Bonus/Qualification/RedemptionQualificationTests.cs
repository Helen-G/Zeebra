using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class RedemptionQualificationTests : BonusTestsBase
    {
        [Test]
        public void Per_player_bonus_issuance_limit_qualification_per_lifetime()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.PlayerRedemptionsLimitType = BonusPlayerRedemptionsLimitType.Lifetime;
            bonus.Template.Availability.PlayerRedemptionsLimit = 1;
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(1, because: "Only first bonus is processed.");
        }

        [TestCase(BonusPlayerRedemptionsLimitType.Day, 1)]
        [TestCase(BonusPlayerRedemptionsLimitType.Week, 7)]
        [TestCase(BonusPlayerRedemptionsLimitType.Month, 31) ]
        public void Per_player_bonus_issuance_limit_qualification_per_period(BonusPlayerRedemptionsLimitType periodType, int daysOffset)
        {
            DateTimeOffset dt = DateTimeOffset.Now;
            SystemTime.Factory = () => dt;
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus();
            
            bonus.ActiveFrom = SystemTime.Now.AddDays(-2);
            bonus.ActiveTo = SystemTime.Now.AddDays(60);
            bonus.Template.Availability.PlayerRedemptionsLimitType = periodType;
            bonus.Template.Availability.PlayerRedemptionsLimit = 2;
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);

            // Switch to the next datetime period
            var dt2 = dt.AddDays(daysOffset);
            SystemTime.Factory = () => dt2;
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(3, because: "Only first 3 bonuses are processed.");
        }

        [TearDown]
        public void CleanUp()
        {
            SystemTime.Factory = () => DateTimeOffset.Now;
        }

        [Test]
        public void Per_bonus_bonus_issuance_limit_qualification()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.RedemptionsLimit = 2;
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(2, because: "Only first 2 bonuses are processed.");
        }

        [Test]
        public void Per_currency_reward_limit_qualification()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardTiers.Single().RewardAmountLimit = bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward;
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(1, because: "Only first bonus is processed.");
        }

        [Test]
        public void Bonus_is_not_issued_if_ParentBonus_is_not_issued()
        {
            var parentBonusRule = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Availability.ParentBonusId = parentBonusRule.Id;

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Player_VIP_level_qualification()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.VipLevels = new List<BonusVip> { new BonusVip { Code = "Bronze" } };

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty(because: "Player has not qualified VIP level");
        }

        [TestCase(true, ExpectedResult = 0)]
        [TestCase(false, ExpectedResult = 1)]
        public int Player_Risk_level_qualification(bool riskLevelActive)
        {
            var riskLevelId = Guid.NewGuid();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            bonus.Template.Availability.ExcludeRiskLevels = new List<RiskLevelExclude> { new RiskLevelExclude { ExcludedRiskLevelId = riskLevelId } };

            var player = BonusRepository.Players.Single(x => x.Id == PlayerId);
            player.RiskLevels = new List<RiskLevel>
            {
                new RiskLevel
                {
                    Id = riskLevelId,
                    IsActive = riskLevelActive
                }
            };

            return BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId).Count();
        }

        [TestCase(0, ExpectedResult = 0)]
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(2, ExpectedResult = 1)]
        [TestCase(3, ExpectedResult = 0)]
        public int List_does_not_contain_bonuses_for_player_with_invalid_registration_date_range(int registeredDaysAgo)
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            var brandNow = DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId);
            bonus.Template.Availability.PlayerRegistrationDateFrom = brandNow.AddDays(-2);
            bonus.Template.Availability.PlayerRegistrationDateTo = brandNow.AddMilliseconds(-1);

            BonusRepository.Players.Single().DateRegistered = brandNow.AddDays(-1 * registeredDaysAgo);

            return BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId).Count();
        }

        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 0)]
        [TestCase(6, ExpectedResult = 0)]
        [TestCase(7, ExpectedResult = 1)]
        [TestCase(8, ExpectedResult = 1)]
        public int List_does_not_contain_bonuses_for_player_with_invalid_within_registration_days(int withinRegistrationDays)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Availability.WithinRegistrationDays = withinRegistrationDays;

            var player = BonusRepository.Players.First();
            player.DateRegistered = player.DateRegistered.AddDays(-7);

            PaymentHelper.MakeDeposit(PlayerId);

            return BonusRedemptions.Count(br => br.ActivationState == ActivationStatus.Activated);
        }

        [Test]
        public void Bonus_is_not_issed_if_no_matching_currency_is_found()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardTiers.Single().CurrencyCode = "ABC";

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Application_to_bonus_is_active_inside_Duration_only()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.DepositKind = DepositKind.Reload;
            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);

            PaymentHelper.MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(11);

            PaymentHelper.MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(9);

            PaymentHelper.MakeDeposit(PlayerId);
            BonusRedemptions.Should().NotBeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now;
        }
    }
}