using System;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Events
{
    public class DepositConfirmed : DepositEvent
    {
        public DepositConfirmed()
        {
        }

        public DepositConfirmed(OfflineDeposit offlineDeposit) : base(offlineDeposit)
        {
            PlayerId = offlineDeposit.PlayerId;
            Amount = offlineDeposit.Amount;
            Remarks = offlineDeposit.Remark;
        }

        public Guid PlayerId { get; set; }
        public decimal Amount { get; set; }
        public string Remarks { get; set; }
    }
}
