using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IRiskLevelCommands : IApplicationService
    {
        void Create(RiskLevel entity);
        void Update(RiskLevel entity);
        void Activate(RiskLevelId id, string remarks);
        void Deactivate(RiskLevelId id, string remarks);
        void Tag(PlayerId playerId, RiskLevelId riskLevel, string description);
        void Untag(PlayerId id, string description);
    }
}
