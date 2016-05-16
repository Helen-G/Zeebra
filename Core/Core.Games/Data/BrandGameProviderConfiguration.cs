using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Game.Data
{
    public class BrandGameProviderConfiguration
    {
        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        public Guid GameProviderId { get; set; }
        public GameProvider GameProvider { get; set; }

        public Guid GameProviderConfigurationId { get; set; }
        public GameProviderConfiguration GameProviderConfiguration { get; set; }
    }
}
