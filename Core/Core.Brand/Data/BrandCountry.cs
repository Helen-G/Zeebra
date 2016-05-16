using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class BrandCountry
    {
        [Key, ForeignKey("Brand"), Column(Order = 0)]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Key, ForeignKey("Country"), Column(Order = 1)]
        public string CountryCode { get; set; }
        public Country Country { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }
}