using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class WalletCommands : IWalletCommands
    {
        private readonly IGameRepository _repository;
        private readonly IEventBus _eventBus;

        public WalletCommands(IGameRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public void Deposit(Guid playerId, decimal amount, string transactionNumber)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, null);

                wallet.Deposit(amount, transactionNumber);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void Withdraw(Guid playerId, decimal amount, string transactionNumber)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, null);

                wallet.Withdraw(amount, transactionNumber);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void TransferFunds(Guid playerId, Guid srcWalletTemplateId, Guid destWalletTemplateId, decimal amount, string transactionNumber)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var sourceWallet = _repository.GetWalletWithUPDLock(playerId, srcWalletTemplateId);
                var destinationWallet = _repository.GetWalletWithUPDLock(playerId, destWalletTemplateId);

                sourceWallet.TransferFundDebit(amount, transactionNumber);
                destinationWallet.TransferFundCredit(amount, transactionNumber);

                _repository.SaveChanges();
                sourceWallet.Events.ForEach(ev => _eventBus.Publish(ev));
                destinationWallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void IssueBonus(Guid playerId, IssuanceParams issuanceParams)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, issuanceParams.WalletTemplateId);

                wallet.IssueBonus(issuanceParams.Target, issuanceParams.Amount);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void SetHasWageringRequirement(Guid playerId, Guid walletTemplateId, bool hasWagering)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, walletTemplateId);

                wallet.SetHasWageringRequirement(hasWagering);

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void AdjustBalances(Guid playerId, AdjustmentParams adjustment)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, null);

                wallet.AdjustBalances(adjustment);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void AdjustBetWonTransaction(Guid playerId, BetWonAdjustmentParams adjustment)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, adjustment.WalletTemplateId);

                wallet.AdjustBetWonTransaction(adjustment);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }
        
        public void Lock(Guid playerId, LockUnlockParams lockParams)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, lockParams.WalletTemplateId);

                wallet.Lock(lockParams.Amount, lockParams.Type, lockParams.Description);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }

        public void Unlock(Guid playerId, LockUnlockParams unlockParams)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var wallet = _repository.GetWalletWithUPDLock(playerId, unlockParams.WalletTemplateId);

                wallet.Unlock(unlockParams.Amount, unlockParams.Type, unlockParams.Description);

                _repository.SaveChanges();
                wallet.Events.ForEach(ev => _eventBus.Publish(ev));
                scope.Complete();
            }
        }
    }
}