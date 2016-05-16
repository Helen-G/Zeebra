using System;

namespace AFT.RegoV2.Core.Common.Data.Player
{
    public class PlayerId
    {
        private readonly Guid _id;

        public PlayerId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PlayerId id)
        {
            return id._id;
        }

        public static implicit operator PlayerId(Guid id)
        {
            return new PlayerId(id);
        }
    }
}