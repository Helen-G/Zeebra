using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Rollover
{
    class RolloverLockUnlockTests : BonusTestsBase
    {
        private Core.Game.Data.Wallet _mainWallet;
        private GamesTestHelper _gamesTestHelper;
        private IGameRepository _walletRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _walletRepository = Container.Resolve<IGameRepository>();
            _mainWallet = _walletRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain);
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Bonus_without_rollover_is_not_locked_on_activation(bool isWithdrawable)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.IsWithdrawable = isWithdrawable;
            bonus.Template.Wagering.HasWagering = false;

            PaymentHelper.MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(0);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Bonus_with_rollover_is_locked_on_activation(bool isWithdrawable)
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.IsWithdrawable = isWithdrawable;
            bonus.Template.Wagering.HasWagering = true;

            PaymentHelper.MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(225);
        }

        [Test]
        public void Lock_is_issued_to_correct_wallets()
        {
            var recievingWallet = _walletRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain == false);
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Info.WalletTemplateId = recievingWallet.Template.Id;

            PaymentHelper.MakeDeposit(PlayerId);

            _mainWallet.BonusLock.Should().Be(200);
            recievingWallet.BonusLock.Should().Be(25);
        }

        [Test]
        public void Lock_is_released_from_correct_wallets_when_rollover_is_finished()
        {
            var productWallet = _walletRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain == false);
            var gameId = _gamesTestHelper.GetGameId(_mainWallet.Template.Id);
            
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = productWallet.Template.Id}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 0.1m;
            bonus.Template.Info.WalletTemplateId = _mainWallet.Template.Id;

            PaymentHelper.MakeDeposit(PlayerId, 100);
            PaymentHelper.MakeFundIn(PlayerId, productWallet.Template.Id, 100);
            _gamesTestHelper.PlaceAndLoseBet(25, PlayerId, gameId);

            _mainWallet.BonusLock.Should().Be(0);
            _mainWallet.Locks
                .SingleOrDefault(tr => tr.Amount == -25)
                .Should()
                .NotBeNull();
            productWallet.BonusLock.Should().Be(0);
            productWallet.Locks
                .SingleOrDefault(tr => tr.Amount == -100)
                .Should()
                .NotBeNull();
        }

        [Test]
        public void Lock_is_released_from_correct_wallets_when_bonus_is_canceled()
        {
            var productWallet = _walletRepository.Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain == false);
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            bonus.Template.Rules.FundInWallets = new List<BonusFundInWallet>
            {
                new BonusFundInWallet {WalletId = productWallet.Template.Id}
            };
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 0.1m;
            bonus.Template.Info.WalletTemplateId = _mainWallet.Template.Id;

            PaymentHelper.MakeDeposit(PlayerId, 100);
            PaymentHelper.MakeFundIn(PlayerId, productWallet.Template.Id, 100);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.Single().Id);

            _mainWallet.BonusLock.Should().Be(0);
            _mainWallet.Locks
                .SingleOrDefault(tr => tr.Amount == -25)
                .Should()
                .NotBeNull();
            productWallet.BonusLock.Should().Be(0);
            productWallet.Locks
                .SingleOrDefault(tr => tr.Amount == -100)
                .Should()
                .NotBeNull();
        }
    }
}