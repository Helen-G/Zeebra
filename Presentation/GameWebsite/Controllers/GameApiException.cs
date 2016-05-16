using System;

namespace AFT.RegoV2.GameWebsite.Controllers
{
    public class GameApiException : Exception
    {
        public GameApiException(string description) : base(description)
        {
        }
    }
}
