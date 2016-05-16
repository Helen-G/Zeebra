using System;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace Core.Player.ApplicationServices
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerQueries _playerQueries;

        public PlayerService(IPlayerQueries playerQueries)
        {
            _playerQueries = playerQueries;
        }

        public string GetVipLevelIdByPlayerId(Guid playerId)
        {
            var vipLevel = _playerQueries.GetPlayer(playerId).VipLevel;
            return vipLevel == null ? "10000000-0000-0000-0000-000000000000" : vipLevel.Id.ToString();
        }
    }
}