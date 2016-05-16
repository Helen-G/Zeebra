using System;

namespace AFT.RegoV2.Core.Game.Exceptions
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