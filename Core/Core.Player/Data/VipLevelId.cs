using System;

namespace AFT.RegoV2.Core.Player.Data
{
    public class VipLevelId
    {
        private readonly Guid _id;

        public VipLevelId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(VipLevelId id)
        {
            return id._id;
        }

        public static implicit operator VipLevelId(Guid id)
        {
            return new VipLevelId(id);
        }
    }
}