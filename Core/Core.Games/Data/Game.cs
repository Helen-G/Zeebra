using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Game.Data
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string EndpointPath { get; set; }

        public Guid GameProviderId { get; set; }
        public GameProvider GameProvider { get; set; }

        public string Type { get; set; }
        public bool IsActive { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }

    }
}
