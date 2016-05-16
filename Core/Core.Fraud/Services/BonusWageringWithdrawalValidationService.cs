using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Fraud.Services
{
    public class BonusWageringWithdrawalValidationService : IBonusWageringWithdrawalValidationService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly WalletQueries _walletQueries;

        public BonusWageringWithdrawalValidationService(
            IPaymentRepository paymentRepository,
            WalletQueries walletQueries)
        {
            _paymentRepository = paymentRepository;
            _walletQueries = walletQueries;
        }

        public void Validate(OfflineWithdrawRequest request)
        {
            var bankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).FirstOrDefault(x => x.Id == request.PlayerBankAccountId);
            
            var playerId = bankAccount.Player.Id;
            if (_walletQueries.PlayerHasWageringRequirement(playerId))
                throw new BonusWageringValidationException();
        }
    }
}