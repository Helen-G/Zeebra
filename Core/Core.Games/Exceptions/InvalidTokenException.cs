using System;

namespace AFT.RegoV2.Core.Game.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() {}
        public InvalidTokenException(string message) : base(message){}
    }
}