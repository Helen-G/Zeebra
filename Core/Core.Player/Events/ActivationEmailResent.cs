using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class ActivationEmailResent : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }

        public ActivationEmailResent() { }

        public ActivationEmailResent(Guid playerId, string email, string token)
        {
            PlayerId = playerId;
            Email = email;
            Token = token;
        }
    }
}
