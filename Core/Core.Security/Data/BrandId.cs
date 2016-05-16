using System;

namespace AFT.RegoV2.Core.Security.Data
{
    public class BrandId
    {
        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public virtual User User { get; set; }
    }
}
