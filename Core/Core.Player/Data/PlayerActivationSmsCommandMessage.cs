using System;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Domain.Player.Events;

namespace AFT.RegoV2.Core.Player.Data
{
    public class PlayerActivationSmsCommandMessage : SmsCommandMessage
    {
        public Guid PlayerId { get; private set; }
        public string Token { get; private set; }

        public PlayerActivationSmsCommandMessage(
            string phoneNumber,
            string body,
            Guid playerId,
            string token) 
            : base(phoneNumber, body)
        {
            PlayerId = playerId;
            Token = token;
        }

        public PlayerActivationSmsCommandMessage()
        {
        }
    }
}