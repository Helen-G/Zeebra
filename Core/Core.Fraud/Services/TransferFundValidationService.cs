using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Fraud
{
    public class TransferFundValidationService : ITransferFundValidationService
    {
        private readonly IBasePaymentQueries _paymentQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly WalletQueries _walletQueries;
        private readonly BrandQueries _brandQueries;

        private const decimal MaxServerDecimal = (decimal) 9999999999999999.99;
        private const bool SettingsActive = true;

        private const int DaysInWeek = 7;
        private const int DaysInMonth = 30;

        public TransferFundValidationService(
            PaymentQueries paymentQueries,
            IPaymentRepository paymentRepository,
            WalletQueries walletQueries,
            BrandQueries brandQueries
            )
        {
            _paymentQueries = paymentQueries;
            _paymentRepository = paymentRepository;
            _walletQueries = walletQueries;
            _brandQueries = brandQueries;
        }

        public TransferFundValidationDTO Validate(TransferFundRequest request)
        {
            var walletTemplate = _brandQueries.GetWalletTemplate(new Guid(request.WalletId));
            if (walletTemplate == null)
                return new TransferFundValidationDTO { ErrorMessage = string.Format("Wallet with id {0} not found.", request.WalletId) };
            
            var timezoneId = walletTemplate.Brand.TimezoneId;
            
            var transferSetting = _paymentQueries.GetTransferSetting(
                request.WalletId,
                request.TransferType,
                SettingsActive);

            var allRequestsFromPlayer =
                _paymentRepository
                    .TransferFunds
                    .Where(x =>
                        x.WalletId == request.WalletId &&
                        x.CreatedBy == request.PlayerId.ToString() &&
                        x.TransferType == request.TransferType &&
                        x.Status == TransferFundStatus.Approved)
                    .ToList();

            if (request.Amount > MaxServerDecimal)
                return new TransferFundValidationDTO { ErrorMessage = "Amount must not contain more than {18} characters." };

            if (request.Amount <= 0)
                return new TransferFundValidationDTO { ErrorMessage = "app:payment.amountMustBeGreaterThanZero" };

            var walletTemplateId = request.TransferType == TransferFundType.FundIn ? null : (Guid?)new Guid(request.WalletId);
            try
            {
                var playerBalance = _walletQueries.GetPlayerBalance(request.PlayerId, walletTemplateId);
                if (request.Amount > playerBalance.Main)
                    return new TransferFundValidationDTO { ErrorMessage = "app:payment.amountExceedsBalance" };
            }
            catch (RegoException)
            {
                return new TransferFundValidationDTO { ErrorMessage = string.Format("Wallet not found for Player with id {0}.", request.PlayerId) };
            }
            
            if (transferSetting != null)
            {
                //Verify amount per transaction
                if ((transferSetting.MaxAmountPerTransaction > 0) && (request.Amount > transferSetting.MaxAmountPerTransaction))
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("The entered amount exceeds the allowed value. Maximum value is {0}.", transferSetting.MaxAmountPerTransaction) };

                if ((transferSetting.MinAmountPerTransaction > 0) && request.Amount < transferSetting.MinAmountPerTransaction)
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("The entered amount is below the allowed value. Minimum value is {0}.", transferSetting.MinAmountPerTransaction) };
                    
                //Verify amount per day
                decimal amount = 0;
                allRequestsFromPlayer
                    .Where(x => (DateTimeOffset.Now.ToBrandOffset(timezoneId) - x.Created) <= TimeSpan.FromDays(1))
                    .ToList()
                    .ForEach(x => amount += x.Amount);

                if (amount + request.Amount > transferSetting.MaxAmountPerDay && transferSetting.MaxAmountPerDay != 0)
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("Total entered amounts exceed the daily limit. Maximum value is {0}.", transferSetting.MaxAmountPerDay) };

                //Verify transactions per day
                var transactionsPerDayCount = allRequestsFromPlayer
                    .Count(x => (DateTimeOffset.Now.ToBrandOffset(timezoneId) - x.Created) 
                        <= TimeSpan.FromDays(1));

                if (transactionsPerDayCount + 1 > transferSetting.MaxTransactionPerDay && transferSetting.MaxTransactionPerDay != 0)
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("Total number of transfers per day exceed daily limit. Maximum value is {0}.", transferSetting.MaxTransactionPerDay) };

                //Verify transactions per week
                var transactionsPerWeekCount = allRequestsFromPlayer
                    .Count(x => (DateTimeOffset.Now.ToBrandOffset(timezoneId) - x.Created)
                        <= TimeSpan.FromDays(DaysInWeek));

                if (transactionsPerWeekCount + 1 > transferSetting.MaxTransactionPerWeek && transferSetting.MaxTransactionPerWeek != 0)
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("Total number of transfers per day exceed weekly limit. Maximum value is {0}.", transferSetting.MaxTransactionPerWeek) };

                //Verify transactions per month
                var transactionsPerMonthCount = allRequestsFromPlayer
                    .Count(x => (DateTimeOffset.Now.ToBrandOffset(timezoneId) - x.Created)
                        <= TimeSpan.FromDays(DaysInMonth));

                if (transactionsPerMonthCount + 1 > transferSetting.MaxTransactionPerMonth && transferSetting.MaxTransactionPerMonth != 0)
                    return new TransferFundValidationDTO { ErrorMessage = string.Format("Total number of transfers per day exceed monthly limit. Maximum value is {0}.", transferSetting.MaxTransactionPerMonth) };
            }

            return new TransferFundValidationDTO{IsValid = true};
        }
    }
}
