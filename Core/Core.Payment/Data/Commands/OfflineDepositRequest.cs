using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Commands
{
    public class OfflineDepositRequest
    {
        public OfflineDepositRequest()
        {
            NotificationMethod = NotificationMethod.Email;
        }

        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public Guid BankAccountId { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public NotificationMethod NotificationMethod { get; set; }

        public string BonusCode { get; set; }

        public string RequestedBy { get; set; }

        public string PlayerRemark { get; set; }
    }
}