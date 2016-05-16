using System;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class RiskLevel
    {
        public Guid Id { get; set; }
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }        
        public virtual Brand Brand { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public string Description { get; set; }
    }
}
