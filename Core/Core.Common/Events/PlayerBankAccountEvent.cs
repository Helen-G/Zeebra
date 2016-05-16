using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events
{
    public abstract class PlayerBankAccountEvent : DomainEventBase
    {
        protected PlayerBankAccountEvent() { }

        protected PlayerBankAccountEvent(
            Guid playerId,
            Guid playerBankAccountId,
            string accountNumber)
        {
            PlayerId = playerId;
            PlayerBankAccountId = playerBankAccountId;
            AccountNumber = accountNumber;
        }

        public Guid PlayerId { get; set; }
        public Guid PlayerBankAccountId { get; set; }
        public string AccountNumber { get; set; }
    }
}
