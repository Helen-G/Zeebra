using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Player
{
    public class PlayerContactVerified : DomainEventBase
    {
        public PlayerContactVerified() { } // default constructor is required for publishing event to MQ

        public PlayerContactVerified(Guid playerId, ContactType contactType)
        {
            PlayerId = playerId;
            ContactType = contactType;
        }
        public Guid PlayerId { get; set; }
        public ContactType ContactType { get; set; }
    }
    public enum ContactType
    { 
        Mobile,
        Email
    }
}
