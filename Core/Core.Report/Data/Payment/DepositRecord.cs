using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Domain.BoundedContexts.Report.Attributes;

namespace AFT.RegoV2.BoundedContexts.Report.Data
{
    public class DepositRecord
    {
        [Key, Export("Reference Code")]
        public Guid DepositId { get; set; }

        [Index, MaxLength(100), Export]
        public string Licensee { get; set; }

        [Index, MaxLength(100), Export]
        public string Brand { get; set; }

        [Index, MaxLength(100), Export]
        public string Username { get; set; }
        
        [Index, Export("Internal Account")]
        public bool IsInternalAccount { get; set; }

        [Index, MaxLength(100),]
        public string VipLevel { get; set; }

        [Index, MaxLength(100), Export("Transaction ID")]
        public string TransactionId { get; set; }

        [Index, MaxLength(100), Export("Payment Method")]
        public string PaymentMethod { get; set; }

        [Index, MaxLength(100), Export]
        public string Currency { get; set; }
        
        [Index, Export]
        public decimal Amount { get; set; }
        
        [Index, Export("Actual Amount")]
        public decimal ActualAmount { get; set; }
        
        [Index, Export]
        public decimal Fee { get; set; }

        [Index, MaxLength(100), Export]
        public string Status { get; set; }
        
        [Index, Export("Date Submitted")]
        public DateTimeOffset Submitted { get; set; }

        [Index, MaxLength(100), Export("Submitted By")]
        public string SubmittedBy { get; set; }
        
        [Index, Export("Date Approved")]
        public DateTimeOffset? Approved { get; set; }

        [Index, MaxLength(100), Export("Approved By")]
        public string ApprovedBy { get; set; }
        
        [Index, Export("Date Rejected")]
        public DateTimeOffset? Rejected { get; set; }

        [Index, MaxLength(100), Export("Rejected By")]
        public string RejectedBy { get; set; }
        
        [Index, Export("Date Verified")]
        public DateTimeOffset? Verified { get; set; }

        [Index, MaxLength(100), Export("Verified By")]
        public string VerifiedBy { get; set; }

        [Index, MaxLength(100), Export("Deposit Type")]
        public string DepositType { get; set; }

        [Index, MaxLength(100), Export("Bank Account Name")]
        public string BankAccountName { get; set; }

        [Index, MaxLength(100), Export("Bank Account ID")]
        public string BankAccountId { get; set; }

        [Index, MaxLength(100), Export("Bank Name")]
        public string BankName { get; set; }

        [Index, MaxLength(100), Export("Province")]
        public string BankProvince { get; set; }

        [Index, MaxLength(100), Export("Branch")]
        public string BankBranch { get; set; }

        [Index, MaxLength(100), Export("Bank Account Number")]
        public string BankAccountNumber { get; set; }
    }
}