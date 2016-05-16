using System;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerBankAccountCurrentSet : PlayerBankAccountEvent
    {
        public PlayerBankAccountCurrentSet() { }

        public PlayerBankAccountCurrentSet(
            Guid playerId,
            Guid playerBankAccountId,
            string accountNumber)
            : base(playerId, playerBankAccountId, accountNumber)
        {
        }
    }
}
