using System;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameProviderConfiguration
    {
        public Guid Id { get; set; }
        public GameProvider GameProvider { get; set; }
        public Guid GameProviderId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Endpoint { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
    }
}