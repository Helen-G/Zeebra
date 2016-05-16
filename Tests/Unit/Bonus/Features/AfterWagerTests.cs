using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class AfterWagerTests : BonusTestsBase
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
            _wallet = gameRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain);

            var gameProviderId = _wallet.Template.WalletTemplateGameProviders.First().GameProviderId;
            _gameId = gameRepository.GameProviders.Single(x => x.Id == gameProviderId).Games.First().Id;

            _bonus = BonusHelper.CreateBasicBonus();
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 1;
            _bonus.Template.Wagering.IsAfterWager = true;
        }

        [Test]
        public void Bonus_redemption_is_Pending_with_Active_rollover_after_fulfilling_qualification()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var bonusRedemption = BonusRepository.GetLockedPlayer(PlayerId).BonusesRedeemed.Single();
            bonusRedemption.ActivationState.Should().Be(ActivationStatus.Pending);
            bonusRedemption.RolloverState.Should().Be(RolloverStatus.Active);
        }

        [Test]
        public void Bonus_reward_is_credited_after_wagering_is_completed()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.BonusLock.Should().Be(200);

            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);

            _wallet.Bonus.Should().Be(25);
            _wallet.BonusLock.Should().Be(0);
        }

        [Test]
        public void ManualByPlayer_bonus_lock_is_released_after_wagering_is_completed()
        {
            _bonus.Template.Info.Mode = IssuanceMode.ManualByPlayer;
            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.BonusLock.Should().Be(200);

            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);

            _wallet.BonusLock.Should().Be(0);
        }

        [Test]
        public void Bonus_reward_is_credited_after_wagering_is_zeroed_out()
        {
            _bonus.Template.Wagering.Multiplier = 10;
            _bonus.Template.Wagering.Threshold = 175;
            PaymentHelper.MakeDeposit(PlayerId);

            _wallet.BonusLock.Should().Be(200);

            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, _gameId);

            _wallet.Bonus.Should().Be(25);
            _wallet.BonusLock.Should().Be(0);
        }
    }
}