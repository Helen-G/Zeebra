using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using FluentValidation;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    internal class IssueByCsValidator : AbstractValidator<IssueBonusByCsVM>
    {
        public IssueByCsValidator(BonusQueries queries, IBonusRepository repository, IWalletQueries walletQueries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            Func<Guid, Data.Bonus> bonusGetter = (bonusId) => queries.GetCurrentVersionBonuses().SingleOrDefault(b => b.Id == bonusId);
            Func<Guid, Player> playerGetter = (playerId) => repository.Players.SingleOrDefault(b => b.Id == playerId);
            Func<Guid, Guid, BonusTransaction> transactionGetter = (playerId, transactionId) =>
                playerGetter(playerId).Wallets.SelectMany(w => w.Transactions).SingleOrDefault(t => t.Id == transactionId);

            RuleFor(model => model.BonusId)
                .Must(id => bonusGetter(id) != null)
                .WithMessage(ValidatorMessages.BonusDoesNotExist);

            RuleFor(model => model.PlayerId)
                .Must(id => playerGetter(id) != null)
                .WithMessage(ValidatorMessages.PlayerDoesNotExist);

            When(model => playerGetter(model.PlayerId) != null && bonusGetter(model.BonusId) != null, () =>
            {
                RuleFor(model => model.TransactionId)
                    .Must((model, transactionId) => transactionGetter(model.PlayerId, transactionId) != null)
                    .WithMessage(ValidatorMessages.TransactionDoesNotExist);

                When(model => transactionGetter(model.PlayerId, model.TransactionId) != null, () => RuleFor(model => model.TransactionId)
                    .Must((model, transactionId) =>
                    {
                        var transaction = transactionGetter(model.PlayerId, transactionId);
                        var bonus = bonusGetter(model.BonusId);

                        if (bonus.Template.Info.BonusTrigger == Trigger.FundIn)
                        {
                            return transaction.Type == TransactionType.FundIn;
                        }
                        if (bonus.Template.Info.BonusTrigger == Trigger.Deposit &&
                            (bonus.Template.Info.DepositKind == DepositKind.First || bonus.Template.Info.DepositKind == DepositKind.Reload))
                        {
                            return transaction.Type == TransactionType.Deposit;
                        }

                        return true;
                    })
                    .WithMessage(ValidatorMessages.TransactionTypeDoesNotMatchBonusType)
                    .Must((model, transactionId) =>
                    {
                        var qualifiedBonuses = queries.GetManualByCsQualifiedBonuses(model.PlayerId);
                        var theQualifedBonus = qualifiedBonuses.SingleOrDefault(b => b.Id == model.BonusId);
                        if (theQualifedBonus == null)
                            return false;
                        var qualifiedTransactions = queries.GetManualByCsQualifiedTransactions(model.PlayerId, model.BonusId);
                        if (qualifiedTransactions.Select(tr => tr.Id).Contains(model.TransactionId) == false)
                            return false;

                        return true;
                    })
                    .WithMessage(ValidatorMessages.PlayerIsNotQualifiedForBonus)
                    .Must((model, transactionId) =>
                    {
                        var bonus = bonusGetter(model.BonusId);
                        if (bonus.Template.Wagering.HasWagering == false)
                            return true;

                        var bonusWallet = playerGetter(model.PlayerId).Wallets.Single(w => w.Transactions.Select(t => t.Id).Contains(transactionId));
                        var transactionWallet = walletQueries.GetPlayerBalance(model.PlayerId, bonusWallet.TemplateId);
                        var transaction = transactionGetter(model.PlayerId, transactionId);

                        return transactionWallet.Main >= transaction.TotalAmount;
                    })
                    .WithMessage(ValidatorMessages.PlayerHasNoFundsToLockLeft)
                    .Must((model, transactionId) =>
                    {
                        var bonus = bonusGetter(model.BonusId);
                        if (bonus.Template.Wagering.HasWagering == false)
                            return true;

                        var wallet = walletQueries.GetPlayerBalance(model.PlayerId, bonus.Template.Info.WalletTemplateId);

                        return wallet.Playable >= bonus.Template.Wagering.Threshold;
                    })
                    .WithMessage(ValidatorMessages.PlayerBalanceIsLessThanWageringThreshold));
            });
        }
    }
}