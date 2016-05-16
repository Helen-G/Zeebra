using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class NotEnoughFundsException : Exception
    {
        public NotEnoughFundsException()
            : base("Entered amount exceeds the current balance.")
        {
            
        }
    }
}
