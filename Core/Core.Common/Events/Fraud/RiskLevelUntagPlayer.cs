using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelUntagPlayer : AFT.RegoV2.Core.Common.Interfaces.DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid RiskLevelId { get; set; }
        public string Description { get; set; }

        public RiskLevelUntagPlayer()
        { }

        public RiskLevelUntagPlayer(Guid id, Guid playerId, Guid riskLevelId, string remarks)
        {
            this.Id = id;
            this.PlayerId = playerId;
            this.RiskLevelId = riskLevelId;
            this.Description = remarks;
        }
    }
}
