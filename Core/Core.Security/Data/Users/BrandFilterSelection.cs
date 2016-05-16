using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Security.Data
{
    public class BrandFilterSelection
    {
        [Key, Column(Order = 0)]
        public Guid UserId { get; set; }
        [Key, Column(Order = 1)]
        public Guid BrandId { get; set; }
        public virtual User User { get; set; }
    }
}
