using System;

namespace AFT.RegoV2.Core.Wallet.Exceptions
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException()
        {    
        }

        public InvalidAmountException(string message) : base(message) 
        {
        }
    }
}