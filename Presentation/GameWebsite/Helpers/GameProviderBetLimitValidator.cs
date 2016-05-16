using System;
using System.Collections.Generic;
using System.Linq;

namespace AFT.RegoV2.GameWebsite.Helpers
{

    public class GameProviderBetLimit
    {
        public string BetLimitId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
    }

    public class GameProviderBetLimitValidator
    {
        private List<GameProviderBetLimit> _betLimits;

        public GameProviderBetLimitValidator(List<GameProviderBetLimit> betLimits)
        {
            _betLimits = betLimits;
        }

        public void Validate(string betLimitId, string currencyCode, decimal betValue)
        {
            var betLimit = _betLimits.SingleOrDefault(x => x.BetLimitId == betLimitId && x.CurrencyCode == currencyCode);

            if (betLimit != null)
            {
                if (betLimit.MaxValue < betValue || betLimit.MinValue > betValue)
                {
                    throw new GameProviderBetLimitException(betLimit);
                }
            }
        }
    }

    public class GameProviderBetLimitException : Exception
    {
        public GameProviderBetLimitException(GameProviderBetLimit betLimit) : base(
            string.Format("Bet out of range. Must be {0}-{1}.", betLimit.MinValue, betLimit.MaxValue))
        {
            
        }
    }
}