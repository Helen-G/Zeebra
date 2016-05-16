using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Brand
    {
        public Brand()
        {
        }

        public Guid     Id { get; set; }
        public Guid     LicenseeId { get; set; }
        public string   Name { get; set; }
        public string   Code { get; set; }
        public string   TimezoneId { get; set; }

        public Licensee                 Licensee { get; set; }
        
        public ICollection<BrandGameProviderConfiguration> BrandGameProviderConfigurations { get; set; }
    }
}
