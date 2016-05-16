using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Lock
    {
        public Lock()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public LockType LockType { get; set; }
        public string Description { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
    }
}