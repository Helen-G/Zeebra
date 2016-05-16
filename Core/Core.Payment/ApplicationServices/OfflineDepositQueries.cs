using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class OfflineDepositQueries : IApplicationService
    {
        private readonly IPaymentRepository _paymentRepository;

        public OfflineDepositQueries(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public IEnumerable<OfflineDeposit> GetPendingDeposits(Guid playerId)
        {
           return  _paymentRepository.OfflineDeposits
                .Where(o => o.PlayerId == playerId)
                .Where(
                    o =>
                        o.Status == OfflineDepositStatus.New || o.Status == OfflineDepositStatus.Processing ||
                        o.Status == OfflineDepositStatus.Unverified || o.Status == OfflineDepositStatus.Verified);
        }

        public OfflineDeposit GetOfflineDeposit(Guid id)
        {
            return _paymentRepository.OfflineDeposits
                .Single(o => o.Id == id);
        }
    }
}
