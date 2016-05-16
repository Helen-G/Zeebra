using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Data
{
    public class WalletTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid BrandId { get; set; }
        public bool IsMain { get; set; }
        public string CurrencyCode { get; set; }
        public virtual GameCurrency Currency { get; set; }
        public bool IsArchived { get; set; }

        public List<WalletTemplateGameProvider> WalletTemplateGameProviders { get; set; }
    }
}
