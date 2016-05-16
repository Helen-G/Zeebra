using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Payment.Events;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus
{
    class CancellationTests : BonusTestsBase
    {
        private Core.Game.Data.Wallet _wallet;
        private Core.Bonus.Data.Bonus _bonus;
        private GamesTestHelper _gamesTestHelper;
        private Guid _gameId;
        public override void BeforeEach()
        {
            base.BeforeEach();

            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wallet = Container.Resolve<IGameRepository>().Wallets.Single(a => a.PlayerId == PlayerId && a.Template.IsMain);
            _bonus = BonusHelper.CreateBasicBonus();
            _bonus.Template.Rules.RewardTiers.Single().BonusTiers.Single().Reward = 100;
            _bonus.Template.Wagering.HasWagering = true;
            _bonus.Template.Wagering.Multiplier = 3m;

            _gameId = BrandHelper.GetMainWalletGameId(PlayerId);
        }

        [Test]
        public void Bonus_cancellation_after_net_loss()
        {
            PaymentHelper.MakeDeposit(PlayerId, 100);
            _gamesTestHelper.PlaceAndWinBet(20, 40, PlayerId, _gameId);
            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            _wallet.Main.Should().Be(70);
            _wallet.Bonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(40);
            transaction.BonusBalanceAmount.Should().Be(-140);
        }

        [Test]
        public void Bonus_cancellation_after_net_win()
        {
            PaymentHelper.MakeDeposit(PlayerId, 300);
            _gamesTestHelper.PlaceAndWinBet(200, 300, PlayerId, _gameId);
            _gamesTestHelper.PlaceAndLoseBet(50, PlayerId, _gameId);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            _wallet.Main.Should().Be(300);
            _wallet.Bonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(250);
            transaction.BonusBalanceAmount.Should().Be(-400);
        }

        [Test]
        public void Can_cancel_a_bonus_that_has_no_wagering_contributions()
        {
            PaymentHelper.MakeDeposit(PlayerId, 300);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            _wallet.Main.Should().Be(300);
            _wallet.Bonus.Should().Be(0);
            var transaction = _wallet.Transactions.SingleOrDefault(tr => tr.Type == TransactionType.BonusCancelled);
            transaction.Should().NotBeNull();
            transaction.MainBalanceAmount.Should().Be(0);
            transaction.BonusBalanceAmount.Should().Be(-100);
        }

        [Test]
        public void Bonus_cancellation_releases_bonus_lock()
        {
            PaymentHelper.MakeDeposit(PlayerId, 300);
            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            _wallet.Locks.Count(tr => tr.Amount < 0)
                .Should()
                .Be(2, "Unlock of deposit, bonus amount");
        }

        [Test]
        public void Bonus_cancellation_creates_Cancellation_wagering_contribution_record()
        {
            PaymentHelper.MakeDeposit(PlayerId, 300);

            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.CancelBonusRedemption(PlayerId, bonusRedemption.Id);

            bonusRedemption.Contributions.SingleOrDefault(c => c.Type == ContributionType.Cancellation)
                .Should()
                .NotBeNull();
        }

        [Test]
        public void Can_not_cancel_a_non_existent_bonus()
        {
            Assert.Throws<RegoException>(() => BonusCommands.CancelBonusRedemption(PlayerId, Guid.NewGuid()));
        }

        [TestCase(RolloverStatus.Active, ExpectedResult = true)]
        [TestCase(RolloverStatus.Completed, ExpectedResult = false)]
        [TestCase(RolloverStatus.None, ExpectedResult = false)]
        [TestCase(RolloverStatus.ZeroedOut, ExpectedResult = false)]
        public bool Status_of_bonus_redemption_is_validated_during_cancellation(RolloverStatus status)
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            bonusRedemption.RolloverState = status;

            try
            {
                BonusCommands.CancelBonusRedemption(PlayerId, bonusRedemption.Id);
            }
            catch (RegoException)
            {
                return false;
            }

            return true;
        }

        [Test]
        public void Bonus_cancellation_sets_activation_status_to_Cancelled()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.CancelBonusRedemption(PlayerId, bonusRedemption.Id);

            Assert.AreEqual(ActivationStatus.Canceled, bonusRedemption.ActivationState);
        }

        [Test]
        public void Bonus_cancellation_sets_rollover_status_to_None()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonusRedemption = BonusRedemptions.First();
            BonusCommands.CancelBonusRedemption(PlayerId, bonusRedemption.Id);

            Assert.AreEqual(RolloverStatus.None, bonusRedemption.RolloverState);
        }

        [Test]
        public void Bonus_cancellation_discards_applied_bonus_statistics_changes()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            Assert.AreEqual(100, _bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(1, _bonus.Statistic.TotalRedemptionCount);

            BonusCommands.CancelBonusRedemption(PlayerId, BonusRedemptions.First().Id);

            Assert.AreEqual(0, _bonus.Statistic.TotalRedeemedAmount);
            Assert.AreEqual(0, _bonus.Statistic.TotalRedemptionCount);
        }
    }
}