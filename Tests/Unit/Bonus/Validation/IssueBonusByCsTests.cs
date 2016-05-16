using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Validation
{
    class IssueBonusByCsTests : BonusTestsBase
    {
        [Test]
        public void Bonus_does_not_exist()
        {
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { PlayerId = PlayerId });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [Test]
        public void Player_does_not_exist()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerDoesNotExist);
        }

        [Test]
        public void Transaction_does_not_exist()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionDoesNotExist);
        }

        [Test]
        public void Deposit_transaction_should_be_passed_with_deposit_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            transaction.Type = TransactionType.FundIn;
            var bonus = BonusHelper.CreateBasicBonus();
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionTypeDoesNotMatchBonusType);
        }

        [Test]
        public void Fundin_transaction_should_be_passed_with_fundin_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Info.BonusTrigger = Trigger.FundIn;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TransactionTypeDoesNotMatchBonusType);
        }

        [Test]
        public void Player_is_not_qualified_for_bonus()
        {
            PaymentHelper.MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus(isActive: false);
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerIsNotQualifiedForBonus);
        }

        [Test]
        public void Transaction_should_occur_in_bonus_activity_date_range()
        {
            PaymentHelper.MakeDeposit(PlayerId, 10);
            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();
            transaction.CreatedOn = bonus.ActiveFrom.AddDays(-1);
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerIsNotQualifiedForBonus);
        }

        [Test]
        public void Player_wallet_should_have_funds_to_lock_for_bonus_with_rollover()
        {
            PaymentHelper.MakeDeposit(PlayerId, 20);

            var gameId = BrandHelper.GetMainWalletGameId(PlayerId);
            Container.Resolve<GamesTestHelper>().PlaceAndLoseBet(10, PlayerId, gameId);

            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 10;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerHasNoFundsToLockLeft);
        }

        [Test]
        public void Player_wallet_should_have_more_funds_than_wagering_threshold_for_bonus_with_rollover()
        {
            PaymentHelper.MakeDeposit(PlayerId);

            var transaction =
                BonusRepository.Players.Single(p => p.Id == PlayerId).Wallets.SelectMany(t => t.Transactions).First();
            var bonus = BonusHelper.CreateBasicBonus();

            //setting receiving wallet, not the one deposit goes to
            bonus.Template.Info.WalletTemplateId = bonus.Template.Info.Brand.WalletTemplates.Last().Id;
            bonus.Template.Wagering.HasWagering = true;
            bonus.Template.Wagering.Multiplier = 10;
            bonus.Template.Wagering.Threshold = 60;
            var result = BonusQueries.GetValidationResult(new IssueBonusByCsVM { BonusId = bonus.Id, PlayerId = PlayerId, TransactionId = transaction.Id });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.PlayerBalanceIsLessThanWageringThreshold);
        }
    }
}