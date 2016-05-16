using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class AVCConfigurationQueries : MarshalByRefObject, IAVCConfigurationQueries
    {
        private readonly IFraudRepository _fraudRepository;
        public AVCConfigurationQueries(IFraudRepository fraudRepository)
        {
            _fraudRepository = fraudRepository;
        }
        public AVCConfigurationDTO GetAutoVerificationCheckConfiguration(Guid id)
        {
            var avcConfiguration = _fraudRepository
                .AutoVerificationCheckConfigurations
                .SingleOrDefault(x => x.Id == id);

            var dto = new AVCConfigurationDTO()
            {
                Id = avcConfiguration.Id,
                Brand = avcConfiguration.BrandId,
                Currency = avcConfiguration.Currency,
                VipLevel = avcConfiguration.VipLevelId,
                HasFraudRiskLevel = avcConfiguration.HasFraudRiskLevel,
                Licensee = avcConfiguration.Brand.LicenseeId,
                HasWinnings = avcConfiguration.HasWinnings,
                WinningRules = avcConfiguration.WinningRules.Select(o => new WinningRuleDTO
                {
                    Id = o.Id,
                    Amount = o.Amount,
                    Period = o.Period,
                    Comparison = o.Comparison,
                    ProductId = o.ProductId,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate
                }),
                RiskLevels = avcConfiguration.AllowedRiskLevels.Select(x => x.Id).AsEnumerable(),
                HasWinLoss = avcConfiguration.HasWinLoss,
                WinLossAmount = avcConfiguration.WinLossAmount,
                WinLossOperator = avcConfiguration.WinLossOperator,
                HasDepositCount = avcConfiguration.HasDepositCount,
                TotalDepositCountAmount = avcConfiguration.TotalDepositCountAmount,
                TotalDepositCountOperator = avcConfiguration.TotalDepositCountOperator,
                HasAccountAge = avcConfiguration.HasAccountAge,
                AccountAge = avcConfiguration.AccountAge,
                AccountAgeOperator = avcConfiguration.AccountAgeOperator,
                HasTotalDepositAmount = avcConfiguration.HasTotalDepositAmount,
                TotalDepositAmount = avcConfiguration.TotalDepositAmount,
                TotalDepositAmountOperator = avcConfiguration.TotalDepositAmountOperator,
                HasWithdrawalCount = avcConfiguration.HasWithdrawalCount,
                TotalWithdrawalCountAmount = avcConfiguration.TotalWithdrawalCountAmount,
                TotalWithdrawalCountOperator = avcConfiguration.TotalWithdrawalCountOperator,
                AllowWithdrawalExemption = avcConfiguration.AllowWithdrawalExemption,
                HasNoRecentBonus = avcConfiguration.HasNoRecentBonus,
                HasCompleteDocuments = avcConfiguration.HasCompleteDocuments,
                HasPaymentLevel = avcConfiguration.HasPaymentLevel
            };
            return dto;
        }

        [Permission(Permissions.View, Module = Modules.AutoVerificationConfiguration)]
        public IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations()
        {
            return _fraudRepository
                .AutoVerificationCheckConfigurations
                .Include(x => x.Brand)
                .ToList();
        }

        public IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations(Guid brandId)
        {
            return _fraudRepository
                .AutoVerificationCheckConfigurations
                .Include(x => x.Brand)
                .Where(x => x.Brand.Id == brandId);
        }
    }
}
