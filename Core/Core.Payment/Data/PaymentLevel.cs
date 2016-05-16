using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class PaymentLevel
    {
        public PaymentLevel()
        {
            BankAccounts = new List<BankAccount>();
        }

        public Guid Id { get; set; }

        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; protected set; }

        public virtual string CurrencyCode { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool EnableOfflineDeposit { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset? DateActivated { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset? DateDeactivated { get; set; }
        public PaymentLevelStatus Status { get; set; }

        public virtual ICollection<BankAccount> BankAccounts { get; set; }
    }
}