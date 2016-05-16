using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Services;

namespace AFT.RegoV2.Core.Fraud
{
    public class OfflineWithdrawalValidationService : IOfflineWithdrawalValidationService
    {
        private readonly List<IWithdrawalValidationService> _withdrawalValidationServices;
        public OfflineWithdrawalValidationService(
            IPaymentSettingsValidationService paymentSettingsValidationService,
            IFundsValidationService fundsValidationService,
            IAWCValidationService awcValidationService,
            IManualAdjustmentWageringValidationService manualAdjustmentWageringValidationService,
            IRebateWageringValidationService rebateWageringValidationService,
            IBonusWageringWithdrawalValidationService bonusWageringWithdrawalValidationService,
            IAVCValidationService avcValidationService)
        {
            _withdrawalValidationServices = new List<IWithdrawalValidationService>()
            {
                paymentSettingsValidationService,
                fundsValidationService,
                awcValidationService,
                manualAdjustmentWageringValidationService,
                rebateWageringValidationService,
                bonusWageringWithdrawalValidationService,
                avcValidationService
            };
        }

        public void Validate(OfflineWithdrawRequest request)
        {
            foreach (var withdrawalValidationService in _withdrawalValidationServices)
            {
                withdrawalValidationService.Validate(request);
            }
        }
    }
}