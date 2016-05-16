using System;

namespace AFT.RegoV2.Domain.Payment.Commands
{    
    public class SetCurrentPlayerBankAccountCommand
    {
        public Guid PlayerBankAccountId { get; set; }
    }
}