using System;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class Identity
    {
        public Identity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}