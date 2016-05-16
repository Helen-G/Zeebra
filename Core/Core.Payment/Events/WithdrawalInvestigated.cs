using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class WithdrawalInvestigated : DomainEventBase
    {
        public WithdrawalInvestigated()
        {
        }

        public WithdrawalInvestigated(OfflineWithdraw offlineWithdraw)
        {
            PlayerId = offlineWithdraw.PlayerBankAccount.Player.Id;
            Investigated = offlineWithdraw.InvestigatedDate ?? DateTimeOffset.Now;
            InvestigatedBy = offlineWithdraw.InvestigatedBy;
            Remarks = offlineWithdraw.Remarks;
        }

        public Guid PlayerId { get; set; }
        public DateTimeOffset Investigated { get; set; }
        public string InvestigatedBy { get; set; }
        public string Remarks { get; set; }
    }
}
