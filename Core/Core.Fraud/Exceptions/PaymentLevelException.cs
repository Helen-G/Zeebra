using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class PaymentLevelException : Exception
    {
        public PaymentLevelException() : base("Player's payment level doesn't match the configured criteria")
        {
        }
    }
}
