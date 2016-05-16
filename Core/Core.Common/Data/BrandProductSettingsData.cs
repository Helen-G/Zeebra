using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Common.Data
{
    public class BrandProductSettingsData
    {
        public Guid BrandId { get; set; }
        public Guid ProductId { get; set; }

        //Bet limits for Brand\Produc pair.
        public BetLevelData[] BetLevels { get; set; }
    }
}