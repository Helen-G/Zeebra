using System;

namespace AFT.RegoV2.Core.Game.Exceptions
{
    public class InvalidPlayerIpInTokenException : Exception
    {
         public InvalidPlayerIpInTokenException() : base("Invalid player IP"){}
    }
}