using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.Tests.Unit.Wallet.Steps
{
    [Binding]
    public class BetSteps : WalletStepsBase
    {
        public BetSteps(SpecFlowContainerFactory factory)
            : base(factory)
        {
            WalletRepository = Container.Resolve<IGameRepository>();
            GameWalletOperations = Container.Resolve<GameWalletOperations>();
        }

        [When(@"I place \$(.*) bet")]
        [Given(@"I place \$(.*) bet")]
        public void WhenIPlaceBet(decimal amount)
        {
            WrapCallInExceptionCatch(() =>
            {
                var roundId = GetOrCreate(RoundIdKey, Guid.NewGuid);
                var gameProvider = WalletRepository.GameProviders.Single(
                    x => x.Id == Wallet.Template.WalletTemplateGameProviders.First().GameProviderId);
                var game = WalletRepository.Games.First(x => x.GameProviderId == gameProvider.Id);
                var gameId = GetOrCreate(GameIdKey, () => game.Id);

                GameWalletOperations.PlaceBet(Wallet.PlayerId, gameId, roundId, amount);
            });
        }

        [Then(@"transaction main balance amount should be \$(.*)")]
        public void ThenTransactionMainBalanceAmountShouldBe(decimal mbAmount)
        {
            var transaction = Get<Transaction>(TransactionKey);

            transaction.MainBalanceAmount.Should().Be(mbAmount);
        }

        [Then(@"transaction bonus balance amount should be \$(.*)")]
        public void ThenTransactionBonusBalanceAmountShouldBe(decimal bbAmount)
        {
            var transaction = Get<Transaction>(TransactionKey);

            transaction.BonusBalanceAmount.Should().Be(bbAmount);
        }

        [Then(@"transaction temporary balance amount should be \$(.*)")]
        public void ThenTransactionTemporaryBalanceAmountShouldBe(decimal tbAmount)
        {
            var transaction = Get<Transaction>(TransactionKey);

            transaction.TemporaryBalanceAmount.Should().Be(tbAmount);
        }


        [Then(@"(.*) BetPlaced transactions should be created")]
        public void ThenBetPlacedTransactionsShouldBeCreated(int transactionsCount)
        {
            Wallet.Transactions.Count(tr => tr.Type == TransactionType.BetPlaced).Should().Be(transactionsCount);
        }

        [When(@"I win \$(.*) from bet")]
        [Given(@"I win \$(.*) from bet")]
        public void WhenIWinFromBet(decimal winAmount)
        {
            var roundId = Get<Guid>(RoundIdKey);
            var gameId = Get<Guid>(GameIdKey);

            WrapCallInExceptionCatch(() => GameWalletOperations.WinBet(Wallet.PlayerId, gameId, roundId, winAmount));
        }

        [When(@"I lose \$(.*) from bet")]
        [Given(@"I lose \$(.*) from bet")]
        public void WhenILoseFromBet(decimal loseAmount)
        {
            var roundId = Get<Guid>(RoundIdKey);
            var gameId = Get<Guid>(GameIdKey);

            WrapCallInExceptionCatch(() => GameWalletOperations.LoseBet(Wallet.PlayerId, gameId, roundId));
        }

        [Given(@"I cancel the (.*) transaction")]
        [When(@"I cancel the (.*) transaction")]
        public void WhenICancelTheBet(TransactionType transactionType)
        {
            var gameId = Get<Guid>(GameIdKey);

            var transaction = Wallet.Transactions.LastOrDefault(tr => tr.Type == transactionType) ?? new Transaction();
            WrapCallInExceptionCatch(() => GameWalletOperations.CancelBet(Wallet.PlayerId, gameId, transaction.Id));
        }

        [Then(@"invalid operation exception should be thrown")]
        public void ThenInvalidOperationExceptionShouldBeThrown()
        {
            Context[InvalidOperationFiredKey].Should().Be(true);
        }

        [Then(@"Temporary balance should be \$(.*)")]
        public void ThenTemporaryBalanceShouldBe(decimal amount)
        {
            Wallet.Temporary.Should().Be(amount);
        }

    }
}