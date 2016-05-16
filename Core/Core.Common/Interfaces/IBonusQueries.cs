using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Bonus;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IBonusQueries : IApplicationService
    {
        IEnumerable<OfflineDepositQualifiedBonus> GetOfflineDepositQualifiedBonuses(Guid playerId);
        List<string> GetDepositQualificationFailures(Guid playerId, string bonusCode, decimal depositAmount);
        List<string> GetFundInQualificationFailures(Guid playerId, string bonusCode, decimal fundInAmount, Guid walletTemplateId);
        string GetBonusName(string bonusCode);
        ClaimableBonusRedemption[] GetClaimableRedemptions(Guid playerId);
    }
}