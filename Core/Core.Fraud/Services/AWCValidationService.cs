using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class AWCValidationService : IAWCValidationService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFraudRepository _fraudRepository;

        public AWCValidationService(
            IPaymentRepository paymentRepository,
            IFraudRepository fraudRepository)
        {
            _paymentRepository = paymentRepository;
            _fraudRepository = fraudRepository;
        }

        public void Validate(OfflineWithdrawRequest request)
        {
            var bankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).FirstOrDefault(x => x.Id == request.PlayerBankAccountId);

            var brandId = bankAccount.Player.BrandId;
            var currency = bankAccount.Player.CurrencyCode;

            var wagerConfiguration = _fraudRepository
                .WagerConfigurations
                .Where(
                    x =>
                        x.BrandId == brandId && 
                        (x.IsServeAllCurrencies || x.CurrencyCode == currency) &&
                        x.IsDepositWageringCheck && 
                        x.IsActive);

            if (!wagerConfiguration.Any())
                return;

            var deposits = _paymentRepository.OfflineDeposits.Include(x => x.Player).Where(x => x.Player.Id == bankAccount.Player.Id && x.Status == OfflineDepositStatus.Approved);

            if (deposits.Any(x => x.DepositWagering != 0))
                throw new AutoWagerCheckException();
        }
    }
}