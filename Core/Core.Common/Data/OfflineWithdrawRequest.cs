using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.Core.Common.Data
{
    public class OfflineWithdrawRequest
    {
        [Required]
        public Guid PlayerBankAccountId { get; set; }

        public decimal Amount { get; set; }

        public string RequestedBy { get; set; }

        [MinLength(1), MaxLength(200)]
        public string Remarks { get; set; }

        public string BankAccountTime { get; set; }

        public string BankTime { get; set; }

        public NotificationType NotificationType { get; set; }
    }
}