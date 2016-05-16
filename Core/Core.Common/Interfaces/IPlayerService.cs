using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IPlayerService : IApplicationService
    {
        string GetVipLevelIdByPlayerId(Guid playerId);
    }
}