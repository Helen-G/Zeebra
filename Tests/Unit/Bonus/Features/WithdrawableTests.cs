using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class WithdrawableTests : BonusTestsBase
    {
        private Core.Game.Data.Wallet _wallet;
        private Core.Bonus.Data.Bonus _bonus;
        private GamesTestHelper _gamesTestHelper;
        private Guid _gameId;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var gameRepository = Container.Resolve<IGameRepository>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wallet = Container.Resolve<IGameRepository>().Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain);
            _bonus = BonusHelper.CreateBasicBonus();
            var gameProviderId = _wallet.Template.WalletTemplateGameProviders.First().GameProviderId;
            _gameId = gameRepository.GameProviders.Single(x => x.Id == gameProviderId).Games.First().Id;

        }

        [TestCase(true)]
        [TestCase(false)]
        public void NotWithdrawable_bonus_is_credited_to_bonus_balance(bool hasRollover)
        {
            _bonus.Template.Info.IsWithdrawable = false;
            _bonus.Template.Wagering.HasWagering = hasRollover;

            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.Main.Should().Be(200);
            _wallet.Bonus.Should().Be(25);
        }

        [Test]
        public void Withdrawable_bonus_without_rollover_is_credited_to_main_balance()
        {
            _bonus.Template.Info.IsWithdrawable = true;
            _bonus.Template.Wagering.HasWagering = false;

            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.Main.Should().Be(225);
        }

        [Test]
        public void Withdrawable_bonus_with_rollover_is_credited_to_bonus_balance()
        {
            _bonus.Template.Info.IsWithdrawable = true;
            _bonus.Template.Wagering.HasWagering = true;

            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.Bonus.Should().Be(25);
        }

        [Test]
        public void NotWithrawable_funds_arent_transferred_to_main_once_wagering_is_completed()
        {
            _bonus.Template.Rules.RewardTiers.First().BonusTiers.First().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Method = WageringMethod.Bonus;
            _bonus.Template.Wagering.Multiplier = 2.75m;
            PaymentHelper.MakeDeposit(PlayerId);

            _gamesTestHelper.PlaceAndLoseBet(275, PlayerId, _gameId);

            _wallet.Main.Should().Be(0m);
            _wallet.Bonus.Should().Be(25m);
        }

        [Test]
        public void Issuing_not_withdrawable_bonus_increases_NotWithdrawableBalance()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRepository.Players.Single().Wallets.First().NotWithdrawableBalance.Should().Be(25);
        }

        [Test]
        public void Issuing_withdrawable_bonus_doesnt_increase_NotWithdrawableBalance()
        {
            _bonus.Template.Info.IsWithdrawable = true;
            PaymentHelper.MakeDeposit(PlayerId);

            BonusRepository.Players.Single().Wallets.First().NotWithdrawableBalance.Should().Be(0);
        }

        [Test]
        public void NotWithrawable_bonus_cancellation_decreases_NotWithdrawableBalance()
        {
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Info.IsWithdrawable = false;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;

            PaymentHelper.MakeDeposit(PlayerId, 100);
            
            _gamesTestHelper.PlaceAndWinBet(20, 40, PlayerId, _gameId);
            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            BonusRepository.Players.Single().Wallets.First().NotWithdrawableBalance.Should().Be(0);
        }
    }
}