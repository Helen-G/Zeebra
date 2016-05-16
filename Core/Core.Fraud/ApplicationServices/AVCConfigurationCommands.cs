using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class AVCConfigurationCommands : MarshalByRefObject, IAVCConfigurationCommands
    {
        private readonly IFraudRepository _fraudRepository;
        private readonly ISecurityProvider _securityProvider;

        public AVCConfigurationCommands(IFraudRepository fraudRepository, ISecurityProvider securityProvider)
        {
            _fraudRepository = fraudRepository;
            _securityProvider = securityProvider;
        }

        #region Public methods


        [Permission(Permissions.Add, Module = Modules.AutoVerificationConfiguration)]
        public void Create(AVCConfigurationDTO data)
        {
            this.ValidateAvcEntity(data, AvcConfigurationDtoQueriesEnum.Create);

            var entity = new AutoVerificationCheckConfiguration
            {
                Id = data.Id,
                HasFraudRiskLevel = data.HasFraudRiskLevel,
                BrandId = data.Brand,
                Brand = _fraudRepository.Brands.First(x => x.Id == data.Brand),
                Currency = data.Currency,
                VipLevelId = data.VipLevel,
                DateCreated = DateTimeOffset.UtcNow,

                HasWinLoss = data.HasWinLoss,
                WinLossAmount = data.WinLossAmount,
                WinLossOperator = data.WinLossOperator,

                HasCompleteDocuments = data.HasCompleteDocuments,

                AllowWithdrawalExemption = data.AllowWithdrawalExemption,

                HasTotalDepositAmount = data.HasTotalDepositAmount,
                TotalDepositAmount = data.TotalDepositAmount,
                TotalDepositAmountOperator = data.TotalDepositAmountOperator,

                HasDepositCount = data.HasDepositCount,
                TotalDepositCountAmount = data.TotalDepositCountAmount,
                TotalDepositCountOperator = data.TotalDepositCountOperator,

                HasAccountAge = data.HasAccountAge,
                AccountAge = data.AccountAge,
                AccountAgeOperator = data.AccountAgeOperator,

                HasWithdrawalCount = data.HasWithdrawalCount,
                TotalWithdrawalCountAmount = data.TotalWithdrawalCountAmount,
                TotalWithdrawalCountOperator = data.TotalWithdrawalCountOperator,

                HasNoRecentBonus = data.HasNoRecentBonus,
                CreatedBy = _securityProvider.User.UserId,

                HasPaymentLevel = data.HasPaymentLevel
            };

            //ToDo: Add validation here
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            if (data.HasFraudRiskLevel)
            {
                //Let's add all risk levels.
                _fraudRepository.RiskLevels.Where(x => x.Brand.Id == data.Brand)
                    .ForEach(x => entity.AllowedRiskLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var riskLevelId in data.RiskLevels)
                {
                    var riskLevel = entity.AllowedRiskLevels.FirstOrDefault(x => x.Id == riskLevelId);
                    if (riskLevel != null)
                        entity.AllowedRiskLevels.Remove(riskLevel);
                }
            }

            if (data.HasPaymentLevel)
            {
                //Let's add all payment levels.
                _fraudRepository.PaymentLevels.Where(x => x.Brand.Id == data.Brand && x.CurrencyCode == data.Currency)
                    .ForEach(x => entity.PaymentLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var paymentLevelId in data.PaymentLevels)
                {
                    var paymentLevel = entity.PaymentLevels.FirstOrDefault(x => x.Id == paymentLevelId);
                    if (paymentLevel != null)
                        entity.PaymentLevels.Remove(paymentLevel);
                }

            }

            if (data.HasWinnings)
            {
                entity.HasWinnings = true;
                foreach (var winningRule in data.WinningRules)
                {
                    var rule = CreateWinningRuleEntity(winningRule, entity);

                    if (winningRule.Period == PeriodEnum.CustomDate)
                    {
                        rule.StartDate = winningRule.StartDate;
                        rule.EndDate = winningRule.EndDate;
                    }

                    entity.WinningRules.Add(rule);
                }
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.Add(entity);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }

        private static WinningRule CreateWinningRuleEntity(WinningRuleDTO winningRule, AutoVerificationCheckConfiguration entity)
        {
            return new WinningRule
            {
                Id = Guid.NewGuid(),
                ProductId = winningRule.ProductId,
                Comparison = winningRule.Comparison,
                Amount = winningRule.Amount,
                Period = winningRule.Period,
                AutoVerificationCheckConfigurationId = entity.Id,
                StartDate = winningRule.StartDate,
                EndDate = winningRule.EndDate
            };
        }


        //[Permission(Permissions.Delete, Module = Modules.AutoVerificationConfiguration)]
        public void Delete(Guid id)
        {
            var avcConfiguratoin = _fraudRepository.AutoVerificationCheckConfigurations.Single(x => x.Id == id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.Remove(avcConfiguratoin);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }


        //[Permission(Permissions.Edit, Module = Modules.AutoVerificationConfiguration)]
        public void Update(AVCConfigurationDTO data)
        {
            this.ValidateAvcEntity(data, AvcConfigurationDtoQueriesEnum.Update);

            var entity = _fraudRepository.AutoVerificationCheckConfigurations.Single(x => x.Id == data.Id);
            entity.HasFraudRiskLevel = data.HasFraudRiskLevel;
            entity.BrandId = data.Brand;
            entity.Brand = _fraudRepository.Brands.First(x => x.Id == data.Brand);
            entity.Currency = data.Currency;
            entity.VipLevelId = data.VipLevel;

            entity.HasPaymentLevel = data.HasPaymentLevel;

            entity.HasWinnings = data.HasWinnings;
            UpdateWinningRules(entity, data);
            // entity.WinningRules = data.WinningRules.Select(Mapper.Map<WinningRule>) as ICollection<WinningRule>;

            entity.HasCompleteDocuments = data.HasCompleteDocuments;

            entity.HasWinLoss = data.HasWinLoss;
            entity.WinLossAmount = data.WinLossAmount;
            entity.WinLossOperator = data.WinLossOperator;

            entity.HasWithdrawalCount = data.HasWithdrawalCount;
            entity.TotalWithdrawalCountAmount = data.TotalWithdrawalCountAmount;
            entity.TotalWithdrawalCountOperator = data.TotalWithdrawalCountOperator;

            entity.HasAccountAge = data.HasAccountAge;
            entity.AccountAge = data.AccountAge;
            entity.AccountAgeOperator = data.AccountAgeOperator;

            entity.HasTotalDepositAmount = data.HasTotalDepositAmount;
            entity.TotalDepositAmount = data.TotalDepositAmount;
            entity.TotalDepositAmountOperator = data.TotalDepositAmountOperator;

            entity.HasDepositCount = data.HasDepositCount;
            entity.TotalDepositCountAmount = data.TotalDepositCountAmount;
            entity.TotalDepositCountOperator = data.TotalDepositCountOperator;

            entity.HasNoRecentBonus = data.HasNoRecentBonus;

            if (data.HasFraudRiskLevel)
            {
                entity.AllowedRiskLevels.Clear();

                //Let's add all risk levels.
                _fraudRepository
                    .RiskLevels
                    .Include(x => x.Brand)
                    .Where(x => x.Brand != null)
                    .Where(x => x.Brand.Id == data.Brand)
                    .Where(x => x.Status == Status.Active)
                    .ForEach(x => entity.AllowedRiskLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var riskLevelId in data.RiskLevels)
                {
                    var riskLevel = entity.AllowedRiskLevels.FirstOrDefault(x => x.Id == riskLevelId);
                    if (riskLevel != null)
                        entity.AllowedRiskLevels.Remove(riskLevel);
                }
            }

            if (data.HasPaymentLevel)
            {
                entity.PaymentLevels.Clear();

                //Let's add all payment levels.
                _fraudRepository
                    .PaymentLevels
                    .Include(x => x.Brand)
                    .Where(x => x.BrandId != null && x.CurrencyCode != null)
                    .Where(x => x.Brand.Id == data.Brand)
                    .Where(x => x.CurrencyCode == data.Currency)
                    .ForEach(x => entity.PaymentLevels.Add(x));

                //Let's remove all ignored via UI
                foreach (var paymentLevelId in data.PaymentLevels)
                {
                    var paymentLevel = entity.PaymentLevels.FirstOrDefault(x => x.Id == paymentLevelId);
                    if (paymentLevel != null)
                        entity.PaymentLevels.Remove(paymentLevel);
                }
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _fraudRepository.AutoVerificationCheckConfigurations.AddOrUpdate(entity);
                _fraudRepository.SaveChanges();
                scope.Complete();
            }
        }

        /// <summary>
        /// The method is used to validate AVC entity depending on the query type
        /// </summary>
        /// <param name="data">The entity populated in the form</param>
        /// <param name="queryType">Type of the query: Create, Update etc.</param>
        private void ValidateAvcEntity(AVCConfigurationDTO data, AvcConfigurationDtoQueriesEnum queryType)
        {
            var validationResult = new AVCConfigurationDTOValidator(_fraudRepository, queryType).Validate(data);

            if (validationResult.IsValid)
                return;

            var validationError = validationResult.Errors.FirstOrDefault();

            if (validationError != null)
                throw new RegoException(validationError.ToString());
        }

        private void UpdateWinningRules(AutoVerificationCheckConfiguration entity, AVCConfigurationDTO data)
        {
            foreach (var rule in entity.WinningRules.ToList())
                _fraudRepository.WinningRules.Remove(rule);

            foreach (var rule in data.WinningRules)
                _fraudRepository.WinningRules.Add(CreateWinningRuleEntity(rule, entity));

            _fraudRepository.SaveChanges();
        }

        #endregion
    }
}