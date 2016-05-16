using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class BrandCountriesAssigned : DomainEventBase
    {
        public BrandCountriesAssigned() { } // default constructor is required for publishing event to MQ

        public BrandCountriesAssigned(Data.Brand brand)
        {
            BrandId = brand.Id;
            Countries = brand.BrandCountries.Select(x => x.Country).ToList();
        }

        public Guid BrandId { get; set; }
        public List<Country> Countries { get; set; }
    }
}
