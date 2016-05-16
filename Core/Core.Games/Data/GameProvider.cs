using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameProvider
    {
        public Guid     Id { get; set; }
        public string   Name { get; set; }
        public bool     IsActive { get; set; }
        
        public GameProviderCategory Category { get; set; }

        public string           SecurityKey { get; set; }
        public DateTimeOffset?  SecurityKeyExpiryTime { get; set; }

        public string AuthorizationClientId { get; set; }
        public string AuthorizationSecret { get; set; }

        public AuthenticationMethod Authentication { get; set; }

        public string           CreatedBy { get; set; }
        public DateTimeOffset   CreatedDate { get; set; }
        public string           UpdatedBy { get; set; }
        public DateTimeOffset?  UpdatedDate { get; set; }

        public ICollection<GameProviderConfiguration>    GameProviderConfigurations { get; set; }
        public ICollection<Game> Games { get; set; }
    }
}
