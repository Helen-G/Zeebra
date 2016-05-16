using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalWagerChecked : DomainEventBase
    {
        public WithdrawalWagerChecked()
        {
        }

        public WithdrawalWagerChecked(OfflineWithdraw offlineWithdraw)
        {
            PlayerId = offlineWithdraw.PlayerBankAccount.Player.Id;
            Checked = DateTimeOffset.Now;
            Remarks = offlineWithdraw.Remarks;
        }

        public Guid PlayerId { get; set; }
        public DateTimeOffset Checked { get; set; }
        public string Remarks { get; set; }
    }
}
