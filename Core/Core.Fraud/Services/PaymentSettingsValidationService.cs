using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Fraud
{
    public class PaymentSettingsValidationService : IPaymentSettingsValidationService
    {
        private readonly IBasePaymentQueries _paymentQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly BrandQueries _brandQueries;

        public PaymentSettingsValidationService(
            PaymentQueries paymentQueries,
            IPaymentRepository paymentRepository,
            BrandQueries brandQueries)
        {
            _paymentQueries = paymentQueries;
            _paymentRepository = paymentRepository;
            _brandQueries = brandQueries;
        }

        public void Validate(OfflineWithdrawRequest request)
        {
            var bankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).FirstOrDefault(x => x.Id == request.PlayerBankAccountId);
            var paymentSetting = _paymentQueries.GetPaymentSetting(
                bankAccount.Player.BrandId,
                bankAccount.Player.CurrencyCode,
                _brandQueries.GetVipLevelViewModel(bankAccount.Player.VipLevelId),
                PaymentType.Withdraw);

            var allRequestsFromPlayer =
                _paymentRepository
                    .OfflineWithdraws
                    .Include(x => x.PlayerBankAccount)
                    .Include(x => x.PlayerBankAccount.Player)
                    .Where(x => x.PlayerBankAccount.Player.Id == bankAccount.Player.Id)
                    .ToList();

            if (paymentSetting == null)
                return;
            //Verify amount per transaction
            if (request.Amount > paymentSetting.MaxAmountPerTransaction && paymentSetting.MaxAmountPerTransaction != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountExceedsAllowedValueError", paymentSetting.MaxAmountPerTransaction);

            if (request.Amount < paymentSetting.MinAmountPerTransaction && paymentSetting.MinAmountPerTransaction != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountBelowAllowedValueError", paymentSetting.MinAmountPerTransaction);

            //Verify amount per day
            decimal amount = 0;
            allRequestsFromPlayer
                .Where(x => x.Created.Date.CompareTo(DateTimeOffset.Now.Date) == 0)
                .ToList()
                .ForEach(x => amount += x.Amount);

            if (amount + request.Amount > paymentSetting.MaxAmountPerDay && paymentSetting.MaxAmountPerDay != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.amountExceedsDailyLimitError", amount + request.Amount);

            //Verify transactions per day
            var transactionsPerDayCount = allRequestsFromPlayer
                .Count(x => x.Created.Date.CompareTo(DateTimeOffset.Now.Date) == 0);

            if (transactionsPerDayCount + 1 > paymentSetting.MaxTransactionPerDay && paymentSetting.MaxTransactionPerDay != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsMonthLimitError", transactionsPerDayCount + 1);

            //Verify transactions per week
            var transactionsPerWeekCount = allRequestsFromPlayer
                .Count(x => (x.Created - DateTimeOffset.Now).TotalDays <= 7);

            if (transactionsPerWeekCount + 1 > paymentSetting.MaxTransactionPerWeek && paymentSetting.MaxTransactionPerWeek != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsWeeklyLimitError", transactionsPerWeekCount + 1);

            //Verify transactions per month
            var transactionsPerMonthCount = allRequestsFromPlayer
                .Count(x => x.Created.Date.Month == DateTimeOffset.Now.Month && x.Created.Date.Year == DateTimeOffset.Now.Year);

            if (transactionsPerMonthCount + 1 > paymentSetting.MaxTransactionPerMonth && paymentSetting.MaxTransactionPerMonth != 0)
                throw new PaymentSettingsViolatedException("app:payment.settings.numberTransactionsExceedsMonthLimitError", transactionsPerMonthCount + 1);
        }
    }
}