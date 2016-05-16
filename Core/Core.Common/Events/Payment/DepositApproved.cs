using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Payment
{
    public class DepositApproved : DomainEventBase
    {
        public DepositApproved()
        {
        }

        public Guid DepositId { get; set; }
        public Guid PlayerId { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Fee { get; set; }
        public DateTimeOffset Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string Remarks { get; set; }
        public decimal DepositWagering { get; set; }
    }
}
