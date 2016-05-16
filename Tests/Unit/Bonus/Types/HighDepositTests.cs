using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Types
{
    class HighDepositTests : BonusTestsBase
    {
        [TestCase(500, 50)]
        [TestCase(600, 50)]
        [TestCase(1000, 150)]
        public void Can_redeem_bonus_with_matched_tier(int depositAmount, int expectedRedemptionsAmount)
        {
            BonusHelper.CreateBonusWithHighDepositTiers(false);
            PaymentHelper.MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.Sum(br => br.Amount).Should().Be(expectedRedemptionsAmount);
        }

        [Test]
        public void Bonus_rewards_are_calculated_correctly_across_several_deposits()
        {
            BonusHelper.CreateBonusWithHighDepositTiers(false);
            PaymentHelper.MakeDeposit(PlayerId, 600);

            var bonusRedemption = BonusRedemptions.SingleOrDefault(br => br.Amount == 50);
            Assert.NotNull(bonusRedemption);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);

            PaymentHelper.MakeDeposit(PlayerId, 400);

            bonusRedemption = BonusRedemptions.SingleOrDefault(br => br.Amount == 100);
            Assert.NotNull(bonusRedemption);
            Assert.AreEqual(ActivationStatus.Activated, bonusRedemption.ActivationState);
        }

        [TestCase(500, 50)]
        [TestCase(600, 50)]
        [TestCase(1000, 100)]
        public void Can_redeem_bonus_with_matched_auto_generate_tier(int depositAmount, int expectedRedemptionsAmount)
        {
            BonusHelper.CreateBonusWithHighDepositTiers();
            PaymentHelper.MakeDeposit(PlayerId, depositAmount);

            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.Sum(br => br.Amount).Should().Be(expectedRedemptionsAmount);
        }

        [Test]
        public void Auto_generated_bonus_rewards_are_calculated_correctly_across_several_deposits()
        {
            BonusHelper.CreateBonusWithHighDepositTiers();
            PaymentHelper.MakeDeposit(PlayerId, 600);
            PaymentHelper.MakeDeposit(PlayerId, 400);

            BonusRedemptions.Count.Should().Be(2);
            BonusRedemptions.All(br => br.ActivationState == ActivationStatus.Activated).Should().BeTrue();
            BonusRedemptions.All(br => br.Amount == 50).Should().BeTrue();
        }

        [Test]
        public void Sms_notification_is_sent_upon_hitting_tier_threshold()
        {
            BonusHelper.CreateBonusWithHighDepositTiers(false);
            PaymentHelper.MakeDeposit(PlayerId, 450);

            Assert.AreEqual(1, ServiceBus.PublishedCommandCount);
        }

        [Test]
        public void Sms_notification_is_sent_upon_hitting_auto_generated_tier_threshold()
        {
            BonusHelper.CreateBonusWithHighDepositTiers();
            PaymentHelper.MakeDeposit(PlayerId, 900);

            Assert.AreEqual(1, ServiceBus.PublishedCommandCount);
        }

        [Test]
        public void Cannot_redeem_bonus_with_unmatched_tiers()
        {
            BonusHelper.CreateBonusWithHighDepositTiers(false);
            PaymentHelper.MakeDeposit(PlayerId, 450);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Cannot_redeem_bonus_with_unmatched_auto_generated_tiers()
        {
            BonusHelper.CreateBonusWithHighDepositTiers();
            PaymentHelper.MakeDeposit(PlayerId, 450);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Reward_threshold_is_calculated_correctly_for_auto_generated_tiers()
        {
            var bonus = BonusHelper.CreateBonusWithHighDepositTiers();
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            PaymentHelper.MakeDeposit(PlayerId, 950);

            var bonusRewardThreshold = new Core.Bonus.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.AreEqual(1000, bonusRewardThreshold.DepositAmountRequired);
            Assert.AreEqual(50, bonusRewardThreshold.BonusAmount);
            Assert.AreEqual(50, bonusRewardThreshold.RemainingAmount);
        }

        [Test]
        public void Reward_threshold_is_calculated_correctly()
        {
            var bonus = BonusHelper.CreateBonusWithHighDepositTiers(false);
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            PaymentHelper.MakeDeposit(PlayerId, 950);

            var bonusRewardThreshold = new Core.Bonus.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.AreEqual(1000, bonusRewardThreshold.DepositAmountRequired);
            Assert.AreEqual(100, bonusRewardThreshold.BonusAmount);
            Assert.AreEqual(50, bonusRewardThreshold.RemainingAmount);
        }

        [Test]
        public void Reward_threshold_is_null_if_player_deposited_more_then_requires_last_tier()
        {
            var bonus = BonusHelper.CreateBonusWithHighDepositTiers(false);
            var player = BonusRepository.GetLockedPlayer(PlayerId);

            PaymentHelper.MakeDeposit(PlayerId, 1100);

            var bonusRewardThreshold = new Core.Bonus.Entities.Bonus(bonus).CalculateRewardThreshold(player);

            Assert.Null(bonusRewardThreshold);
        }

        [Test]
        public void Redemptions_are_not_created_if_all_reward_tiers_were_redeemed_this_month()
        {
            BonusHelper.CreateBonusWithHighDepositTiers(false);
            PaymentHelper.MakeDeposit(PlayerId, 1000);
            PaymentHelper.MakeDeposit(PlayerId, 1000);

            BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Count.Should().Be(2);
        }
    }
}