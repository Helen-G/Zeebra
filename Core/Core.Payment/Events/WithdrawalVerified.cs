using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalVerified : DomainEventBase
    {
        public WithdrawalVerified() { } 

        public WithdrawalVerified(OfflineWithdraw offlineWithdraw)
        {
            PlayerId = offlineWithdraw.PlayerBankAccount.Player.Id;
            VerifiedBy = offlineWithdraw.VerifiedBy;
            Verified = DateTime.Now;
            Remarks = offlineWithdraw.Remarks;
        }

        public Guid PlayerId { get; set; }
        public DateTime Verified { get; set; }
        public string VerifiedBy { get; set; }
        public string Remarks { get; set; }
    }
}
