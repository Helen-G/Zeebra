using System;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Domain.Payment.Events
{
    public class DepositUnverified : DomainEventBase
    {
        public DepositUnverified()
        {
        }

        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public OfflineDepositStatus Status { get; set; }
        public DateTime Cancelled { get; set; }
        public string CancelledBy { get; set; }
        public string Remarks { get; set; }
        public string UnverifyReason { get; set; }
    }
}
