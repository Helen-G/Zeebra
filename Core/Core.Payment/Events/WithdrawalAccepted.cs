using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalAccepted : DomainEventBase
    {
        public WithdrawalAccepted()
        {
        }

        public WithdrawalAccepted(OfflineWithdraw offlineWithdraw)
        {
            PlayerId = offlineWithdraw.PlayerBankAccount.Player.Id;
            Accepted = offlineWithdraw.AcceptedTime ?? DateTimeOffset.Now;
            AcceptedBy = offlineWithdraw.AcceptedBy;
            Remarks = offlineWithdraw.Remarks;
        }

        public Guid PlayerId { get; set; }
        public DateTimeOffset Accepted { get; set; }
        public string AcceptedBy { get; set; }
        public string Remarks { get; set; }
    }
}
