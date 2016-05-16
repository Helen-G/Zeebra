using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IRiskLevelQueries : IApplicationService
    {
        RiskLevel GetById(Guid id);
        IQueryable<RiskLevel> GetAll();
        IQueryable<RiskLevel> GetByBrand(Guid brandId);
        IQueryable<PlayerRiskLevel> GetAllPlayerRiskLevels();
        IQueryable<PlayerRiskLevel> GetPlayerRiskLevels(Guid playerId);
    }
}
