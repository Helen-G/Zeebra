using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Domain.Payment.Events;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Types
{
    class ReloadAndFirstDepositTests : BonusTestsBase
    {
        [Test]
        public void Percentage_deposit_bonus_reward_calculated_correctly()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(5000);
        }

        [Test]
        public void Flat_deposit_bonus_reward_calculated_correctly()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            //Adding a reward for CNY currency. This should not be used, 'cos Player's currency is CAD
            bonus.Template.Rules.RewardTiers.Add(new RewardTier
            {
                CurrencyCode = "CNY",
                BonusTiers = new List<TierBase>
                {
                    new BonusTier
                    {
                        Reward = 100
                    }
                }
            });

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(25);
        }

        [Test]
        public void First_deposit_bonus_works()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.DepositKind = DepositKind.First;

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Last().Amount.Should().Be(25);
        }

        [Test]
        public void First_deposit_bonus_is_applied_to_first_deposit_only()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            BonusHelper.CreateBasicBonus();

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Reload_deposit_bonus_is_applied_to_second_and_sunsequent_deposits()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId);
            BonusRedemptions.Should().BeEmpty();

            PaymentHelper.MakeDeposit(PlayerId);
            BonusRedemptions.Should().NotBeEmpty();
        }

        [Test]
        public void Bonus_redemtion_amount_is_recalculated_if_deposit_amount_changes()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 0.5m;

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 200,
                DepositId = depositId
            });
            ServiceBus.PublishMessage(new DepositApproved
            {
                PlayerId = PlayerId,
                ActualAmount = 100,
                DepositId = depositId
            });

            BonusRedemptions.First().Amount.Should().Be(50);
            bonus.Statistic.TotalRedeemedAmount.Should().Be(50);
        }

        [TestCase(100, 10)]
        [TestCase(200, 40)]
        [TestCase(300, 50)]
        public void Can_redeem_deposit_bonus_with_matched_percentage_specific_rules(int depositAmount,
            int expectedRedemptionAmount)
        {
            var bonus = BonusHelper.CreateBonusWithBonusTiers(BonusRewardType.TieredPercentage);

            PaymentHelper.MakeDeposit(PlayerId, depositAmount);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(PlayerId, bonusRedemption.Player.Id);
            Assert.AreEqual(bonus.Id, bonusRedemption.Bonus.Id);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(expectedRedemptionAmount, bonusRedemption.Amount);
        }

        [TestCase(0.2, BonusRewardType.TieredPercentage, TestName = "Cannot redeem percentage tiered deposit bonus with invalid deposit amount")]
        [TestCase(0.9, BonusRewardType.TieredAmount, TestName = "Cannot redeem flat amount tiered deposit bonus with invalid deposit amount")]
        public void Cannot_redeem_deposit_bonus(decimal depositAmount, BonusRewardType rewardType)
        {
            BonusHelper.CreateBonusWithBonusTiers(rewardType);

            PaymentHelper.MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.Should().BeEmpty();
        }

        [TestCase(100, 10)]
        [TestCase(200, 20)]
        [TestCase(500, 30)]
        public void Can_redeem_deposit_bonus_with_matched_fixed_amount_specific_rules(int depositAmount,
            int expectedRedemptionAmount)
        {
            var bonus = BonusHelper.CreateBonusWithBonusTiers(BonusRewardType.TieredAmount);

            PaymentHelper.MakeDeposit(PlayerId, depositAmount);

            var bonusRedemption = BonusRedemptions.First();

            Assert.AreEqual(PlayerId, bonusRedemption.Player.Id);
            Assert.AreEqual(bonus.Id, bonusRedemption.Bonus.Id);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
            Assert.AreEqual(expectedRedemptionAmount, bonusRedemption.Amount);
        }

        [Test]
        public void Cancel_of_deposit_negates_related_bonus_redemption()
        {
            BonusHelper.CreateBasicBonus();

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 100,
                DepositId = depositId
            });
            ServiceBus.PublishMessage(new DepositUnverified
            {
                PlayerId = PlayerId,
                DepositId = depositId
            });

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Negated);
        }

        [Test]
        public void Cancel_of_deposit_discards_applied_bonus_statistics_changes()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 0.5m;

            var depositId = Guid.NewGuid();
            ServiceBus.PublishMessage(new DepositSubmitted
            {
                PlayerId = PlayerId,
                Amount = 100,
                DepositId = depositId
            });

            Assert.AreEqual(50, bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(1, bonus.Statistic.TotalRedemptionCount);

            ServiceBus.PublishMessage(new DepositUnverified
            {
                PlayerId = PlayerId,
                DepositId = depositId
            });

            Assert.AreEqual(0, bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(0, bonus.Statistic.TotalRedemptionCount);
        }
    }
}