using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IWalletCommands : IApplicationService
    {
        void Deposit(Guid playerId, decimal amount, string transactionNumber);
        void Withdraw(Guid playerId, decimal amount, string transactionNumber);

        void IssueBonus(Guid playerId, IssuanceParams issuanceParams);
        void SetHasWageringRequirement(Guid playerId, Guid walletTemplateId, bool hasWagering);
        void AdjustBalances(Guid playerId, AdjustmentParams adjustment);
        void AdjustBetWonTransaction(Guid playerId, BetWonAdjustmentParams adjustment);

        void Lock(Guid playerId, LockUnlockParams lockParams);
        void Unlock(Guid playerId, LockUnlockParams unlockParams);

        void TransferFunds(Guid playerId, Guid srcWalletTemplateId, Guid destWalletTemplateId, decimal amount, string transactionNumber);
    }
}