using System;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class ActivationLinkSent : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public ContactType ContactType { get; set; }
        public string Token { get; set; }

        public ActivationLinkSent() { }

        public ActivationLinkSent(Guid playerId, ContactType contactType, string token)
        {
            PlayerId = playerId;
            ContactType = contactType;
            Token = token;
        }
    }
}
