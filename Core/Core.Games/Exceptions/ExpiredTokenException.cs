using System;

namespace AFT.RegoV2.Core.Game.Exceptions
{
    public class ExpiredTokenException : Exception
    {
        public ExpiredTokenException() : base("Expired token"){}
    }
}