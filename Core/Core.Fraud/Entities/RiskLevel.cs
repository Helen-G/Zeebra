using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Fraud.Entities
{
    public class RiskLevel
    {
        //private readonly Data.RiskLevel _data;
        internal List<IDomainEvent> Events = new List<IDomainEvent>();

        public RiskLevel()
        { }

        public RiskLevel(Data.RiskLevel data)
        { }

        public void TagPlayer(Guid id, Guid playerId, Guid riskLevel, string description)
        {
            Events.Add(new RiskLevelTagPlayer(id, playerId, riskLevel, description));
        }

        public void UntagPlayer(Guid id, Guid playerId, Guid riskLevel, string description)
        {
            Events.Add(new RiskLevelUntagPlayer(id, playerId, riskLevel, description));
        }

    }
}
