using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class IssuanceModeTests : BonusTestsBase
    {
        [Test]
        public void System_activates_Automatic_mode_bonus_redemption()
        {
            BonusHelper.CreateBasicBonus();

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_activates_AutomaticWithBonusCode_mode_bonus_redemption()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.First().ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void Player_should_activate_ManualByPlayer_mode_bonus_redemption()
        {
            BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);

            PaymentHelper.MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_redeems_both_Automatic_and_ManualByPlayer_deposit_bonus_when_bonus_code_isnot_provided()
        {
            BonusHelper.CreateBasicBonus();

            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);

            var bonus3 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Count.Should().Be(2);
            BonusRedemptions.Any(r => r.Bonus.Id == bonus3.Id).Should().BeFalse("Bonus 3 requires bonus code to be passed with deposit.");
        }

        [Test]
        public void System_redeems_both_AutomaticWithBonusCode_deposit_bonus_when_bonus_code_provided()
        {
            BonusHelper.CreateBasicBonus();

            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);

            var bonus3 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus3.Code);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus3.Id);
        }

        [Test]
        public void System_redeems_both_Automatic_and_ManualByPlayer_fundin_bonus_when_bonus_code_isnot_provided()
        {
            var bonus1 = BonusHelper.CreateBasicBonus();
            bonus1.Template.Info.BonusTrigger = Trigger.FundIn;
            var brandWalletId = bonus1.Template.Info.Brand.WalletTemplates.First().Id;
            bonus1.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus2.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus2.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus3 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus3.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus3.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, brandWalletId, 200);

            BonusRedemptions.Count.Should().Be(2);
            BonusRedemptions.Any(r => r.Bonus.Id == bonus3.Id).Should().BeFalse("Bonus 3 requires bonus code to be passed with fund in.");
        }

        [Test]
        public void System_redeems_both_AutomaticWithBonusCode_fundin_bonus_when_bonus_code_provided()
        {
            var bonus1 = BonusHelper.CreateBasicBonus();
            bonus1.Template.Info.BonusTrigger = Trigger.FundIn;
            var brandWalletId = bonus1.Template.Info.Brand.WalletTemplates.First().Id;
            bonus1.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus2.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus2.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            var bonus3 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus3.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus3.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };

            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, brandWalletId, 200, bonus3.Code);

            BonusRedemptions.Count.Should().Be(1);
            BonusRedemptions.First().Bonus.Id.Should().Be(bonus3.Id);
        }

        [Test]
        public void Cannot_redeem_bonus_providing_non_existent_bonusCode()
        {
            PaymentHelper.MakeDeposit(PlayerId, bonusCode: TestDataGenerator.GetRandomString());

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Can_not_redeem_bonus_by_code_if_it_does_not_require_it()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Code = TestDataGenerator.GetRandomString();

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void Can_not_redeem_bonus_of_invalid_type_by_code()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;

            PaymentHelper.MakeDeposit(PlayerId, 200, bonus.Code);

            BonusRedemptions.Should().BeEmpty(because: "Bonus code is corresponded with fund-in bonus.");
        }

        [Test]
        public void Can_redeem_bonus_providing_correct_bonusCode()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions
                .Count(br => br.ActivationState == ActivationStatus.Activated)
                .Should().Be(1);
        }

        [Test]
        public void Can_claim_percentage_reward_type_bonus()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().Reward = 0.5m;

            PaymentHelper.MakeDeposit(PlayerId, 1000);

            var bonusRedemption = BonusRedemptions.Single();
            Assert.DoesNotThrow(() => BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id));
            bonusRedemption.Amount.Should().Be(500);
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Activated);
        }

        [Test]
        public void System_does_not_redeem_ManualByCS_bonus_automatically()
        {
            BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);

            PaymentHelper.MakeDeposit(PlayerId);

            BonusRedemptions.Should().BeEmpty();
        }

        [Test]
        public void System_does_not_redeem_ManualByCS_bonus_by_bonus_code()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus.Code);

            BonusRedemptions.Should().BeEmpty();
        }
    }
}