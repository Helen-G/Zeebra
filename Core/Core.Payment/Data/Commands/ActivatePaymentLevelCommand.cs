using System;

namespace AFT.RegoV2.Core.Payment.Data.Commands
{
    public class ActivatePaymentLevelCommand
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}
