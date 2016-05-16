using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Tests.Common.Base;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Rollover
{
    class RolloverLockAmountCalculationTests : BonusTestsBase
    {
        [TestCase(true, ExpectedResult = 200, TestName = "Deposit after wager bonus locks deposit only")]
        [TestCase(false, ExpectedResult = 225, TestName = "Deposit before wager bonus locks deposit and bonus")]
        public decimal Deposit_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.Deposit;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            PaymentHelper.MakeDeposit(PlayerId);

            return BonusRedemptions.First().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 100, TestName = "Fund-in after wager bonus locks deposit only")]
        [TestCase(false, ExpectedResult = 125, TestName = "Fund-in before wager bonus locks deposit and bonus")]
        public decimal Fundin_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            var brandWalletId = bonus.Template.Info.Brand.WalletTemplates.First().Id;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = brandWalletId}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            PaymentHelper.MakeDeposit(PlayerId, 100);
            PaymentHelper.MakeFundIn(PlayerId, brandWalletId, 100);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "Referral after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 10, TestName = "Referral before wager bonus locks bonus")]
        public decimal Referral_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var referTier = new BonusTier
            {
                From = 1,
                To = 1,
                Reward = 10
            };

            var walletTemplate = BrandHelper.GetMainWalletTemplate();
            var gameId = GamesHelper.GetGameId(walletTemplate);

            var bonus = BonusHelper.CreateBonusWithReferFriendTiers();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Clear();
            bonus.Template.Rules.RewardTiers.Single().BonusTiers.Add(referTier);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            BonusHelper.CompleteReferAFriendRequirments(PlayerId, gameId);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "High deposit after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 50, TestName = "High deposit before wager bonus locks bonus")]
        public decimal High_deposit_bonus_lock_amount_is_correct(bool isAfterWager)
        {
            var bonus = BonusHelper.CreateBonusWithHighDepositTiers(false);
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            PaymentHelper.MakeDeposit(PlayerId, 600);

            return BonusRedemptions.Single().LockedAmount;
        }

        [TestCase(true, ExpectedResult = 0, TestName = "Verification after wager bonus locks nothing")]
        [TestCase(false, ExpectedResult = 25, TestName = "Verification before wager bonus locks bonus")]
        public decimal Verification_bonus_redemption_locks_bonus_only(bool isAfterWager)
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByPlayer);
            bonus.Template.Info.BonusTrigger = Trigger.MobilePlusEmailVerification;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.IsAfterWager = isAfterWager;

            var player = PlayerHelper.CreatePlayer();
            var bonusRedemption = BonusRepository.Players.Single(p => p.Id == player.Id).Wallets.First().BonusesRedeemed.First();
            ServiceBus.PublishLocal(new PlayerContactVerified(player.Id, ContactType.Mobile));
            ServiceBus.PublishLocal(new PlayerContactVerified(player.Id, ContactType.Email));
            if(isAfterWager == false)
                BonusCommands.ClaimBonusRedemption(player.Id, bonusRedemption.Id);

            return bonusRedemption.LockedAmount;
        }
    }
}