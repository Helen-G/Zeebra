using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class GameEndpoint
    {
        public Guid Id { get; set; }
        public GameServer GameServer { get; set; }
        public Guid GameServerId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}