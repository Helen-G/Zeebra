using System;

namespace AFT.RegoV2.Core.Common.Events
{
    public class PlayerBankAccountRejected : PlayerBankAccountEvent
    {
        public PlayerBankAccountRejected() { }

        public PlayerBankAccountRejected(
            Guid playerId,
            Guid playerBankAccountId,
            string accountNumber,
            string rejectedBy, 
            DateTimeOffset rejected,
            DateTimeOffset created,
            string remarks)
            : base(playerId, playerBankAccountId, accountNumber)
        {
            RejectedBy = rejectedBy;
            Rejected = rejected;
            Created = created;
            Remarks = remarks;
        }

        public string RejectedBy { get; set; }
        public DateTimeOffset Rejected { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Remarks { get; set; }
    }
}