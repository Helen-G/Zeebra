using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events
{
    public class WithdrawalCancelled : DomainEventBase
    {
        public WithdrawalCancelled() { } // default constructor is required for publishing event to MQ

        public WithdrawalCancelled( string cancelledBy)
        {
            Cancelled = DateTime.Now;
            CancelledBy = cancelledBy;
        }

        public Guid PlayerId { get; set; }
        public WithdrawalStatus Status { get; set; }
        public DateTime Cancelled { get; set; }
        public string CancelledBy { get; set; }
        public string Remarks { get; set; }
        public decimal Amount { get; set; }
    }
}
