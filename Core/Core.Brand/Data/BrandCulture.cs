using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Core.Common.Brand.Data;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class BrandCulture
    {
        [Key, ForeignKey("Brand"), Column(Order = 0)]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Key, ForeignKey("Culture"), Column(Order = 1)]
        public string CultureCode { get; set; }
        public Culture Culture { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public string AddedBy { get; set; }
    }
}