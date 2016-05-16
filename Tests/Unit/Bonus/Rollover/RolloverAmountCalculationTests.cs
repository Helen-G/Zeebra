using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Rollover
{
    class RolloverAmountCalculationTests : BonusTestsBase
    {
        [TestCase(WageringMethod.Bonus, ExpectedResult = 50, TestName = "Bonus only rollover is calculated correctly")]
        [TestCase(WageringMethod.TransferAmount, ExpectedResult = 400, TestName = "Deposit only rollover is calculated correctly")]
        [TestCase(WageringMethod.BonusAndTransferAmount, ExpectedResult = 450, TestName = "Deposit+bonus rollover is calculated correctly")]
        public decimal Bonus_rollover_is_calculated_correctly(WageringMethod method)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = method;
            bonus.Template.Wagering.Multiplier = 2;

            PaymentHelper.MakeDeposit(PlayerId);

            return BonusRedemptions.First().Rollover;
        }

        [Test]
        public void Bonus_rollover_is_calculated_based_on_actual_transfer_and_bonus_amounts()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 0.5m;
            bonus.Template.Info.IsWithdrawable = true;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.BonusAndTransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            var offlineDeposit = PaymentHelper.CreateOfflineDeposit(PlayerId, 200);
            PaymentHelper.ConfirmOfflineDeposit(offlineDeposit);
            PaymentHelper.VerifyOfflineDeposit(offlineDeposit, true);

            offlineDeposit.Amount = 150;
            PaymentHelper.ApproveOfflineDeposit(offlineDeposit, true);

            BonusRedemptions
                .First()
                .Rollover
                .Should()
                .Be(450, "(150 deposit amount + 75 bonus amount) * 2 Multiplier");
        }

        [Test]
        public void Deposit_ManualByPlayer_bonus_rollover_is_calculated()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.TransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            PaymentHelper.MakeDeposit(PlayerId);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption.Rollover.Should().Be(400);
        }

        [Test]
        public void Fundin_ManualByPlayer_bonus_rollover_is_calculated()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Method = WageringMethod.TransferAmount;
            bonus.Template.Wagering.Multiplier = 2;

            //depositing funds to use them for fund in
            PaymentHelper.MakeDeposit(PlayerId);
            PaymentHelper.MakeFundIn(PlayerId, brandWalletId, 100);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.ClaimBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption.Rollover.Should().Be(200);
        }
    }
}