using System;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class PlayerPaymentLevel
    {
        public Guid PlayerId { get; set; }
        public PaymentLevel PaymentLevel { get; set; }
    }
}