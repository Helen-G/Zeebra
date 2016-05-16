using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class GameServerDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

        public GameServerCategory Category { get; set; }
    }
}