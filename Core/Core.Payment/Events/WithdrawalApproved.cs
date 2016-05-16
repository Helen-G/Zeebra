using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalApproved : DomainEventBase
    {
        public WithdrawalApproved()
        {
        }

        public WithdrawalApproved(OfflineWithdraw offlineWithdraw)
        {
            PlayerId = offlineWithdraw.PlayerBankAccount.Player.Id;
            Amount = offlineWithdraw.Amount;
            Remarks = offlineWithdraw.Remarks;
            Approved = offlineWithdraw.Approved ?? DateTimeOffset.Now;
            ApprovedBy = offlineWithdraw.ApprovedBy;
        }

        public Guid PlayerId { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string Remarks { get; set; }
    }
}
