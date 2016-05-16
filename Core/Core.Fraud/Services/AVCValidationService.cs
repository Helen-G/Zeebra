using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Security.Cryptography;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using TransactionType = AFT.RegoV2.Core.Player.Data.TransactionType;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class AVCValidationService : IAVCValidationService
    {
        #region Fields

        private readonly IAVCConfigurationQueries _avcConfigurationQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly IWinningRuleQueries _winningRuleQueries;
        private readonly IGameQueries _gameQueries;
        private readonly WalletQueries _walletQueries;
        private readonly IPlayerIdentityValidator _identityValidator;

        #endregion

        #region Constructors

        public AVCValidationService(
            IAVCConfigurationQueries avcConfigurationQueries,
            IPaymentRepository paymentRepository,
            IRiskLevelQueries riskLevelQueries,
            IWinningRuleQueries winningRuleQueries,
            IGameQueries gameQueries,
            WalletQueries walletQueries,
            IPlayerRepository playerRepository,
            IPlayerIdentityValidator identityValidator)
        {
            _avcConfigurationQueries = avcConfigurationQueries;
            _paymentRepository = paymentRepository;
            _riskLevelQueries = riskLevelQueries;
            _winningRuleQueries = winningRuleQueries;
            _gameQueries = gameQueries;
            _walletQueries = walletQueries;
            _identityValidator = identityValidator;
        }

        #endregion

        #region Public methods

        public void Validate(OfflineWithdrawRequest request)
        {
            try
            {
                var bankAccount =
                    _paymentRepository.PlayerBankAccounts.Include(x => x.Player)
                        .FirstOrDefault(x => x.Id == request.PlayerBankAccountId);

                var brandId = bankAccount.Player.BrandId;
                ValidatePlayersFraudRiskLevel(brandId, bankAccount.Player.Id);
                ValidatePlayersWinningsRules(brandId, bankAccount.Player.Id);
                ValidateWinLossRule(brandId, bankAccount.Player.Id, request.Amount);
                ValidateTotalDepositAmount(brandId, bankAccount.Player.Id);
                ValidateTotalWithdrawalCount(brandId, bankAccount.Player.Id);
                ValidateDepositCount(brandId, bankAccount.Player.Id);
                ValidateAccountAge(bankAccount.Player, brandId);
                ValidatePaymentLevel(bankAccount.Player);
                ValidateHasCompleteDocuments(bankAccount.Player, brandId);
            }
            catch (FraudlentRiskException e)
            {
                throw e;
            }
        }

        #endregion

        #region Private methods

        private IList<AutoVerificationCheckConfiguration> GetAllConfigurations(Guid brandId)
        {
            return _avcConfigurationQueries.GetAutoVerificationCheckConfigurations(brandId).ToList();
        }

        private DateTimeOffset? GetEndDate(WinningRule rule)
        {
            var now = DateTimeOffset.Now;
            switch (rule.Period)
            {
                case PeriodEnum.Last7Days:
                case PeriodEnum.Last14Days:
                case PeriodEnum.CurrentYear:
                case PeriodEnum.FromSignUp:
                    return now;
                case PeriodEnum.CustomDate:
                    return rule.EndDate;
            }
            return null;
        }

        private DateTimeOffset? GetStartDate(WinningRule rule, Guid playerId)
        {
            var now = DateTimeOffset.Now;
            switch (rule.Period)
            {
                case PeriodEnum.Last7Days:
                    return now.AddDays(-7);
                case PeriodEnum.Last14Days:
                    return now.AddDays(-14);
                case PeriodEnum.CurrentYear:
                    return new DateTime(now.Year, 1, 1);
                case PeriodEnum.FromSignUp:
                    var player = _paymentRepository.Players.Single(o => o.Id == playerId);
                    return player.DateRegistered.LocalDateTime;
                case PeriodEnum.CustomDate:
                    return rule.StartDate;
            }
            return null;
        }

        private decimal GetTotalWinLoss(Guid playerId, decimal amount)
        {
            var totalDepositAmount = _paymentRepository
                .OfflineDeposits
                .Where(x => x.Status == OfflineDepositStatus.Approved)
                .Where(x => x.PlayerId == playerId)
                .Select(x => x.Amount)
                .Sum();
            var totalWithdrawalAmount = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Include(x => x.PlayerBankAccount.Player)
                .Where(x => x.Status == WithdrawalStatus.Approved)
                .Where(x => x.PlayerBankAccount.Player.Id == playerId)
                .Select(x => x.Amount)
                .DefaultIfEmpty(0)
                .Sum();
            var mainBalance = _walletQueries.GetPlayerBalance(playerId).Main;

            return totalDepositAmount - (totalWithdrawalAmount + mainBalance + amount);
        }

        private bool IsRulePassed(ComparisonEnum rule, decimal ruleAmount, decimal actualAmount)
        {
            switch (rule)
            {
                case ComparisonEnum.Greater:
                    return ruleAmount < actualAmount;
                case ComparisonEnum.Less:
                    return ruleAmount > actualAmount;
                case ComparisonEnum.GreaterOrEqual:
                    return ruleAmount <= actualAmount;
                case ComparisonEnum.LessOrEqual:
                    return ruleAmount >= actualAmount;
            }

            return false;
        }

        private bool IsRulePassed(ComparisonEnum rule, int ruleAmount, int actualAmount)
        {
            switch (rule)
            {
                case ComparisonEnum.Greater:
                    return ruleAmount < actualAmount;
                case ComparisonEnum.Less:
                    return ruleAmount > actualAmount;
                case ComparisonEnum.GreaterOrEqual:
                    return ruleAmount <= actualAmount;
                case ComparisonEnum.LessOrEqual:
                    return ruleAmount >= actualAmount;
            }

            return false;
        }

        private void ValidateHasCompleteDocuments(Payment.Data.Player player, Guid brandId)
        {
            var allConfigurations = GetAllConfigurations(brandId);

            var configurations = allConfigurations
                .Where(configuration => configuration.HasCompleteDocuments);

            if (!configurations.Any())
                return;

            _identityValidator.Validate(player.Id, TransactionType.Withdraw);
        }

        private void ValidateAccountAge(Payment.Data.Player player, Guid brandId)
        {
            var daysFromRegistration = (player.DateRegistered - DateTimeOffset.Now).Days * -1;
            var allConfigurations = GetAllConfigurations(brandId);
            foreach (var c in allConfigurations)
            {
                if (c.HasAccountAge)
                {
                    switch (c.AccountAgeOperator)
                    {
                        case ComparisonEnum.Greater:
                            if (daysFromRegistration <= c.AccountAge)
                                throw new AccountAgeException();
                            break;
                        case ComparisonEnum.GreaterOrEqual:
                            if (daysFromRegistration < c.AccountAge)
                                throw new AccountAgeException();
                            break;
                        case ComparisonEnum.Less:
                            if (daysFromRegistration >= c.AccountAge)
                                throw new AccountAgeException();
                            break;
                        case ComparisonEnum.LessOrEqual:
                            if (daysFromRegistration > c.AccountAge)
                                throw new AccountAgeException();
                            break;
                    }
                }
            }
        }

        private void ValidateDepositCount(Guid brandId, Guid playerId)
        {
            var allConfigurations = GetAllConfigurations(brandId);
            var totalDepositAmount = _paymentRepository
                .OfflineDeposits
                .Count(x => x.PlayerId == playerId && x.Status == OfflineDepositStatus.Approved);
            foreach (var c in allConfigurations)
            {
                if (!c.HasDepositCount)
                    continue;
                if (!IsRulePassed(c.TotalDepositCountOperator, c.TotalDepositCountAmount, totalDepositAmount))
                    throw new DepositCountException();
            }
        }

        private void ValidatePlayersFraudRiskLevel(Guid brandId, Guid playerId)
        {
            var allConfigurations = GetAllConfigurations(brandId);
            var allPlayerRiskLevels = _riskLevelQueries.GetPlayerRiskLevels(playerId).ToList();

            var fraudlentRiskLevels =
                allConfigurations.Where(x => x.HasFraudRiskLevel).SelectMany(x => x.AllowedRiskLevels).ToList();
            if (allPlayerRiskLevels.Any(x => fraudlentRiskLevels.Any(fRl => fRl.Id == x.RiskLevel.Id)))
                throw new FraudlentRiskException();
        }

        private void ValidatePlayersWinningsRules(Guid brandId, Guid playerId)
        {
            var allConfigurations = GetAllConfigurations(brandId);

            var allWinningRules = allConfigurations
                .Where(configuration => configuration.HasWinnings)
                .SelectMany(configuration => _winningRuleQueries.GetWinningRules(configuration.Id));

            foreach (var rule in allWinningRules)
            {
                var startDate = GetStartDate(rule, playerId);
                var endDate = GetEndDate(rule);

                var actualTransactions = _gameQueries.GetWinLossGameActions(rule.ProductId)
                    .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate);

                var wonAmount = 0.0m;
                var lostAmount = 0.0m;

                if (actualTransactions.Any())
                {
                    wonAmount = actualTransactions.Where(o => o.GameActionType == GameActionType.Won).Sum(o => o.Amount);
                    lostAmount = actualTransactions.Where(o => o.GameActionType == GameActionType.Lost).Sum(o => o.Amount);
                }

                var winningsAmount = wonAmount - lostAmount;
                if (!IsRulePassed(rule.Comparison, rule.Amount, winningsAmount))
                    throw new HasWinningsAmountExeption();
            }
        }

        private void ValidateTotalDepositAmount(Guid brandId, Guid playerId)
        {
            var totalDepositAmount = _paymentRepository
                .OfflineDeposits
                .Where(x => x.Status == OfflineDepositStatus.Approved)
                .Where(x => x.PlayerId == playerId)
                .Select(x => x.Amount)
                .Sum();

            var avcWithTotalDepositAmount = _avcConfigurationQueries
                .GetAutoVerificationCheckConfigurations(brandId)
                .Where(x => x.HasTotalDepositAmount);

            foreach (var autoVerificationCheckConfiguration in avcWithTotalDepositAmount)
            {
                if (!IsRulePassed(
                    autoVerificationCheckConfiguration.TotalDepositAmountOperator,
                    autoVerificationCheckConfiguration.TotalDepositAmount,
                    totalDepositAmount))
                {
                    throw new TotalDepositAmountException();
                }
            }
        }

        private void ValidateTotalWithdrawalCount(Guid brandId, Guid playerId)
        {
            var allConfigurations = GetAllConfigurations(brandId);
            var widthdrawalCount = _paymentRepository
                .OfflineWithdraws
                .Include(x => x.PlayerBankAccount)
                .Count(x => x.PlayerBankAccount.Player.Id == playerId && x.Status == WithdrawalStatus.Approved);
            foreach (var c in allConfigurations)
            {
                if (!c.HasWithdrawalCount)
                    continue;
                if (!IsRulePassed(c.TotalWithdrawalCountOperator, c.TotalWithdrawalCountAmount, widthdrawalCount))
                    throw new WithdrawalCountException();
            }
        }

        private void ValidateWinLossRule(Guid brandId, Guid playerId, decimal amount)
        {
            var allWithWinLossCriteria = GetAllConfigurations(brandId).Where(x => x.HasWinLoss);
            var totalWinLoss = GetTotalWinLoss(playerId, amount);
            foreach (var autoVerificationCheckConfiguration in allWithWinLossCriteria)
            {
                if (!IsRulePassed(
                    autoVerificationCheckConfiguration.WinLossOperator,
                    autoVerificationCheckConfiguration.WinLossAmount,
                    totalWinLoss))
                {
                    throw new WinLossException();
                }
            }
        }

        private void ValidatePaymentLevel(Payment.Data.Player player)
        {
            //var paymentLevelsForAPlayer =
            //    from playerPaymentLevel in _paymentRepository.PlayerPaymentLevels
            //    join paymentLevel in _paymentRepository.PaymentLevels on playerPaymentLevel.PaymentLevel.Id equals paymentLevel.Id
            //    where       playerPaymentLevel.PlayerId == player.Id 
            //            &&  paymentLevel.BrandId == player.BrandId 
            //            &&  paymentLevel.CurrencyCode == player.CurrencyCode
            //    select new {PlayerID = playerPaymentLevel.PlayerId, PaymentLevelID = paymentLevel.Id};

            //if (!paymentLevelsForAPlayer.ToArray().Any())
            //    throw new PaymentLevelException();
        }


        #endregion
    }
}
