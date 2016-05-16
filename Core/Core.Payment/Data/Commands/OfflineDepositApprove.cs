using System;
using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Domain.Payment.Commands
{
    public class OfflineDepositApprove
    {
        [Required]
        public Guid Id { get; set; }

        [Range(0.01, int.MaxValue)]
        public decimal ActualAmount { get; set; }

        public decimal Fee { get; set; }

        [MaxLength(200)]
        public string PlayerRemark { get; set; }

        [MaxLength(200)]
        public string Remark { get; set; }
    }
}