using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Security.Data
{
    public class LicenseeFilterSelection
    {
        [Key, Column(Order = 0)]
        public Guid UserId { get; set; }
        [Key, Column(Order = 1)]
        public Guid LicenseeId { get; set; }
        public virtual User User { get; set; }
    }
}
