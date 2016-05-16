using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerVipLevelChanged : DomainEventBase
    {
        #region Properties

        public Guid PlayerId { get; set; }
        public Guid VipLevelId { get; set; }

        #endregion

        #region Constructors

        public PlayerVipLevelChanged()
        {
        }

        public PlayerVipLevelChanged(Guid playerId, Guid vipLevelId)
        {
            PlayerId = playerId;
            VipLevelId = vipLevelId;
        }

        #endregion
    }
}