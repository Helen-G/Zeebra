using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Common.Events.Fraud
{
    public class RiskLevelTagPlayer : Interfaces.DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public Guid RiskLevelId { get; set; }
        public string Description { get; set; }

        public RiskLevelTagPlayer()
        { }

        public RiskLevelTagPlayer(Guid id)
        {
            this.Id = id;
        }

        public RiskLevelTagPlayer(Guid id, Guid playerId, Guid riskLevelId, string description)
            : this(id)
        {
            this.PlayerId = playerId;
            this.RiskLevelId = riskLevelId;
            this.Description = description;
        }
    }
}
