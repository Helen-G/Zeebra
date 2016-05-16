using System;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Events
{
    public class DepositVerified : DepositEvent
    {
        public DepositVerified()
        {
        }

        public DepositVerified(OfflineDeposit offlineDeposit) : base(offlineDeposit)
        {
            PlayerId = offlineDeposit.PlayerId;
            Verified = offlineDeposit.Verified ?? DateTimeOffset.Now;
            VerifiedBy = offlineDeposit.VerifiedBy;
            Remarks = offlineDeposit.Remark;
        }

        public Guid PlayerId { get; set; }
        public DateTimeOffset Verified { get; set; }
        public string VerifiedBy { get; set; }
        public string Remarks { get; set; }
    }
}
