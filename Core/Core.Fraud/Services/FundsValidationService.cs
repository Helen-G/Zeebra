using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Fraud
{
    public class FundsValidationService : IFundsValidationService
    {
        #region Fields

        private readonly IPaymentRepository _paymentRepository;
        private readonly WalletQueries _walletQueries;

        #endregion

        #region Constructors

        public FundsValidationService(
            IPaymentRepository paymentRepository,
            WalletQueries walletQueries)
        {
            _paymentRepository = paymentRepository;
            _walletQueries = walletQueries;
        }

        #endregion

        #region IFundsValidationService Members

        public void Validate(OfflineWithdrawRequest request)
        {
            var bankAccount =
                _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .FirstOrDefault(x => x.Id == request.PlayerBankAccountId);

            var wallet = _walletQueries.GetPlayerBalance(bankAccount.Player.Id);

            if (wallet.Free < request.Amount)
            {
                throw new NotEnoughFundsException();
            }
        }

        #endregion
    }
}