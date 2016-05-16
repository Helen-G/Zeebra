using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class OfflineDepositValidator : IOfflineDepositValidator
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;

        public OfflineDepositValidator()
        {
        }

        public OfflineDepositValidator(IPaymentQueries paymentQueries, IPaymentRepository repository)
        {
            _paymentQueries = paymentQueries;
            _repository = repository;
        }

        public void ValidatePaymentSetting(Guid playerId, Guid bankAccountId, decimal amount)
        {
            var player = _paymentQueries.GetPlayer(playerId);

            var vipLevelId = player.VipLevelId == Guid.Empty
                ? "10000000-0000-0000-0000-000000000000"
                : player.VipLevelId.ToString();

            // TODO VipLevel from player
            // var vipLevelId = _playerQueries.GetVipLevelIdByPlayerId(player.Id);
            var gatewaySettings = _paymentQueries.GetGatewaySettings(player.BrandId, PaymentType.Deposit, vipLevelId,
                bankAccountId);

            if (gatewaySettings == null)
                return;

            if (amount > gatewaySettings.MaxAmountPerTransaction && gatewaySettings.MaxAmountPerTransaction > 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountExceedsAllowedValueError", gatewaySettings.MaxAmountPerTransaction);

            if (amount < gatewaySettings.MinAmountPerTransaction && gatewaySettings.MinAmountPerTransaction > 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountBelowAllowedValueError", gatewaySettings.MinAmountPerTransaction);

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.StartOfWeek();
            var startOfDay = now.Date;

            var offlineDeposits = _repository.OfflineDeposits
                .Where(x => x.Status == OfflineDepositStatus.Approved
                            && x.Approved >= startOfMonth
                            && x.PlayerId == playerId)
                .ToList();

            var amountPerDay = offlineDeposits
                .Where(x => x.Approved >= startOfDay && x.PlayerId == playerId)
                .Sum(x => x.ActualAmount) + amount;

            if (amountPerDay > gatewaySettings.MaxAmountPerDay && gatewaySettings.MaxAmountPerDay > 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountExceedsDailyLimitError", gatewaySettings.MaxAmountPerDay);

            if (gatewaySettings.MaxTransactionPerMonth > 0 && offlineDeposits.Count >= gatewaySettings.MaxTransactionPerMonth)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsMonthLimitError", gatewaySettings.MaxTransactionPerMonth);

            var count = offlineDeposits.Count(x => x.Approved >= startOfWeek);

            if (gatewaySettings.MaxTransactionPerWeek > 0 && count >= gatewaySettings.MaxTransactionPerWeek)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsWeeklyLimitError", gatewaySettings.MaxTransactionPerWeek);

            count = offlineDeposits.Count(x => x.Approved >= startOfDay);

            if (gatewaySettings.MaxTransactionPerDay > 0 && count >= gatewaySettings.MaxTransactionPerDay)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsDailyLimitError", gatewaySettings.MaxTransactionPerDay);
        }
    }

    public interface IOfflineDepositValidator
    {
        void ValidatePaymentSetting(Guid playerId, Guid bankAccountId, decimal amount);
    }
}
