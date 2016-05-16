using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.Tests.Unit.Wallet.Steps
{
    [Binding]
    public class BonusSteps : WalletStepsBase
    {
        public BonusSteps(SpecFlowContainerFactory factory)
            : base(factory)
        {
            WalletRepository = Container.Resolve<IGameRepository>();
            WalletCommands = Container.Resolve<WalletCommands>();
        }

        [When(@"I issued \$(.*) bonus to (Bonus|Main) balance")]
        [Given(@"I issued \$(.*) bonus to (Bonus|Main) balance")]
        public void WhenIIssuedBonusToBalance(decimal amount, BalanceTarget balanceTarget)
        {
            WhenIIssuedBonusToWalletBalanceWithoutWageringRequirement(amount, 1, balanceTarget);
        }

        [When(@"I issued \$(.*) bonus to wallet \#(.*) (Bonus|Main) balance")]
        public void WhenIIssuedBonusToWalletBalanceWithoutWageringRequirement(decimal amount, int walletIndex, BalanceTarget balanceTarget)
        {
            var wallet = Wallets.ElementAt(walletIndex - 1);
            var issuanceParams = new IssuanceParams
            {
                Amount = amount,
                Target = balanceTarget,
                WalletTemplateId = wallet.Template.Id
            };
            WrapCallInExceptionCatch(() => WalletCommands.IssueBonus(Wallet.PlayerId, issuanceParams));
        }

        [Then(@"Bonus balance should be \$(.*)")]
        public void ThenBonusBalanceShouldBe(decimal bonusBalance)
        {
            Wallet.Bonus.Should().Be(bonusBalance);
        }

        [Then(@"application exception should be thrown")]
        public void ThenRegoExceptionShouldBeThrown()
        {
            Context[RegoExceptionFiredKey].Should().Be(true);
        }

        [When(@"system adjusts Main for (.*)\$, Bonus for (.*)\$, Temporary for (.*)\$ because of (.*) reason")]
        public void WhenSystemAdjustsMainForBonusForTemporaryForBecauseOfBonusCancelledReason(decimal mainAdjustment, decimal bonusAdjustment, decimal tempAdjustment, AdjustmentReason reason)
        {
            var adjustment = new AdjustmentParams(reason)
            {
                MainBalanceAdjustment = mainAdjustment,
                BonusBalanceAdjustment = bonusAdjustment,
                TemporaryBalanceAdjustment = tempAdjustment
            };
            WrapCallInExceptionCatch(() => WalletCommands.AdjustBalances(Wallet.PlayerId, adjustment));
        }

        [Given(@"wallet has active wagering requirement")]
        public void GivenWalletHasActiveWageringRequirement()
        {
            WalletCommands.SetHasWageringRequirement(Wallet.PlayerId, Wallet.Template.Id, true);
        }

        [When(@"system adjusts transaction to be during rollover")]
        public void WhenSystemAdjustsTransactionToBeDuringRollover()
        {
            WalletCommands.AdjustBetWonTransaction(Wallet.PlayerId, new BetWonAdjustmentParams
            {
                BetWonDuringRollover = true,
                WalletTemplateId = Wallet.Template.Id,
                RelatedTransactionId = Wallet.Transactions.Last(tr => tr.Type == TransactionType.BetWon).Id
            });
        }

        [When(@"system adjusts transaction to not be during rollover")]
        public void WhenSystemAdjustsTransactionToNotBeDuringRollover()
        {
            WalletCommands.AdjustBetWonTransaction(Wallet.PlayerId, new BetWonAdjustmentParams
            {
                BetWonDuringRollover = false,
                WalletTemplateId = Wallet.Template.Id,
                RelatedTransactionId = Wallet.Transactions.Last(tr => tr.Type == TransactionType.BetWon).Id
            });
        }
    }
}