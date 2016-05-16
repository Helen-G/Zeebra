using System;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerBankAccountVerified : PlayerBankAccountEvent
    {
        public PlayerBankAccountVerified() { }

        public PlayerBankAccountVerified(
            Guid playerId,
            Guid playerBankAccountId,
            string accountNumber,
            string verifiedBy, 
            DateTimeOffset verified,
            DateTimeOffset created,
            string remarks)
            : base(playerId, playerBankAccountId, accountNumber)
        {
            VerifiedBy = verifiedBy;
            Verified = verified;
            Created = created;
            Remarks = remarks;
        }
      
        public string VerifiedBy { get; set; }
        public DateTimeOffset Verified { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Remarks { get; set; }
    }
}
