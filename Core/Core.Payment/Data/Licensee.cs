using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class Licensee
    {
        public Licensee()
        {
            Brands = new List<Brand>();
            Currencies = new List<Currency>();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Brand> Brands { get; set; }
        public ICollection<Currency> Currencies { get; set; }
    }
}

