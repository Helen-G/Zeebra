using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Unit.Wallet.Steps
{
    public abstract class WalletStepsBase : SpecFlowStepsTestBase
    {
        protected Core.Game.Data.Wallet Wallet { get { return WalletRepository.Wallets.First(); } }
        protected List<Core.Game.Data.Wallet> Wallets { get { return WalletRepository.Wallets.ToList(); } }
        protected WalletCommands WalletCommands;
        protected IGameWalletOperations GameWalletOperations;
        protected IGameRepository WalletRepository;
        protected const string InvalidAmountFiredKey = "InvalidAmountException";
        protected const string InsufficientFundsFiredKey = "InsufficientFundsException";
        protected const string ArgumentOutOfRangeFiredKey = "ArgumentOutOfRangeException";
        protected const string RegoExceptionFiredKey = "RegoException";
        protected const string InvalidOperationFiredKey = "InvalidOperationException";
        protected const string InvalidUnlockAmountFiredKey = "InvalidUnlockAmountException";
        protected const string RoundIdKey = "roundId";
        protected const string GameIdKey = "gameId";
        protected const string TransactionKey = "transaction";

        protected void WrapCallInExceptionCatch(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidAmountException)
            {
                Context[InvalidAmountFiredKey] = true;
            }
            catch (InsufficientFundsException)
            {
                Context[InsufficientFundsFiredKey] = true;
            }
            catch (ArgumentOutOfRangeException)
            {
                Context[ArgumentOutOfRangeFiredKey] = true;
            }
            catch (RegoException)
            {
                Context[RegoExceptionFiredKey] = true;
            }
            catch (InvalidOperationException)
            {
                Context[InvalidOperationFiredKey] = true;
            }
            catch (InvalidUnlockAmount)
            {
                Context[InvalidUnlockAmountFiredKey] = true;
            }
        }


        protected WalletStepsBase(SpecFlowContainerFactory factory)
            : base(factory)
        {
        }
    }
}