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
    public class LockUnlockSteps : WalletStepsBase
    {
        public LockUnlockSteps(SpecFlowContainerFactory factory)
            : base(factory)
        {
            WalletRepository = Container.Resolve<IGameRepository>();
            WalletCommands = Container.Resolve<WalletCommands>();
        }

        [Then(@"(.*) transaction should be created")]
        public void ThenTransactionShouldBeCreated(TransactionType trType)
        {
            ThenBonusBonusTransactionShouldBeCreatedInWallet(trType, 1);
        }

        [Then(@"no (.*) transaction should be created")]
        public void ThenNoTransactionShouldBeCreated(TransactionType trType)
        {
            var wallet = Wallets.ElementAt(0);
            var transaction = wallet.Transactions.SingleOrDefault(tr => tr.Type == trType);

            transaction.Should().BeNull();
        }

        [Then(@"(.*) transaction should be created in wallet \#(.*)")]
        public void ThenBonusBonusTransactionShouldBeCreatedInWallet(TransactionType trType, int walletIndex)
        {
            var wallet = Wallets.ElementAt(walletIndex - 1);
            var transaction = wallet.Transactions.SingleOrDefault(tr => tr.Type == trType);
            Set(TransactionKey, transaction);

            transaction.Should().NotBeNull();
        }

        [Then(@"\$(.*) (.*) lock record should be created")]
        public void ThenLockRecordShouldBeCreated(decimal amount, LockType type)
        {
            var wallet = Wallets.ElementAt(0);
            var @lock = wallet.Locks.SingleOrDefault(tr => tr.Amount == amount && tr.LockType == type);

            @lock.Should().NotBeNull();
        }

        [Then(@"\$(.*) (.*) unlock record should be created")]
        public void ThenUnlockRecordShouldBeCreated(decimal amount, LockType type)
        {
            ThenLockRecordShouldBeCreated(-amount, type);
        }

        [Then(@"Invalid amount exception is thrown")]
        public void ThenInvalidAmountExceptionIsThrown()
        {
            Context[InvalidAmountFiredKey].Should().Be(true);
        }

        [Then(@"Insufficient funds exception is thrown")]
        public void ThenInsufficientFundsExceptionIsThrown()
        {
            Context[InsufficientFundsFiredKey].Should().Be(true);
        }

        [Then(@"Invalid unlock amount exception is thrown")]
        public void ThenInvalidUnlockAmountExceptionIsThrown()
        {
            Context[InvalidUnlockAmountFiredKey].Should().Be(true);
        }


        [When(@"I apply \$(.*) (.*) lock")]
        [Given(@"I apply \$(.*) (.*) lock")]
        public void WhenILockFromDomain(decimal amount, LockType type)
        {
            var lockParams = new LockUnlockParams
            {
                Amount = amount,
                Type = type
            };
            WrapCallInExceptionCatch(() => WalletCommands.Lock(Wallet.PlayerId, lockParams));
        }

        [Then(@"Argument is out of range exception is thrown")]
        public void ThenArgumentIsOutOfRangeExceptionIsThrown()
        {
            Context[ArgumentOutOfRangeFiredKey].Should().Be(true);
        }

        [Then(@"(Fraud|Withdrawal|Bonus) lock should be \$(.*)")]
        public void ThenLockShouldBe(string lockName, decimal expectedAmount)
        {
            ThenWalletBonusLockShouldBe(1, lockName, expectedAmount);
        }

        [Then(@"wallet \#(.*) (Fraud|Withdrawal|Bonus) lock should be \$(.*)")]
        public void ThenWalletBonusLockShouldBe(int walletIndex, string lockName, decimal expectedAmount)
        {
            var wallet = Wallets.ElementAt(walletIndex - 1);
            var lockAmount = 0m;
            if (lockName == "Fraud")
            {
                lockAmount = wallet.FraudLock;
            }
            else if (lockName == "Withdrawal")
            {
                lockAmount = wallet.WithdrawalLock;
            }
            else if (lockName == "Bonus")
            {
                lockAmount = wallet.BonusLock;
            }

            lockAmount.Should().Be(expectedAmount);
        }

        [Given(@"I apply \$(.*) (.*) unlock")]
        [When(@"I apply \$(.*) (.*) unlock")]
        public void WhenIUnlockFromDomain(decimal amount, LockType type)
        {
            var unlockParams = new LockUnlockParams
            {
                Amount = amount,
                Type = type
            };
            WrapCallInExceptionCatch(() => WalletCommands.Unlock(Wallet.PlayerId, unlockParams));
        }
    }
}