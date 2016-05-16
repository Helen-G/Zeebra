using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class Bank
    {
        public Guid Id { get; set; }
        public string BankId { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; protected set; }
        public ICollection<BankAccount> Accounts { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string UpdatedBy { get; set; }
        public string Remark { get; set; }
    }
}
