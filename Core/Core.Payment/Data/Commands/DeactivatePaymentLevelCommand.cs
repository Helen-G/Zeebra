using System;

namespace AFT.RegoV2.Core.Payment.Data.Commands
{
    public class DeactivatePaymentLevelCommand
    {
        public Guid Id { get; set; }
        public Guid? NewPaymentLevelId { get; set; }
        public string Remarks { get; set; }
    }
}
