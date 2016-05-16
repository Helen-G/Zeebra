using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class WalletTemplate
    {
        public WalletTemplate()
        {
            WalletTemplateProducts = new List<WalletTemplateProduct>();
        }

        public Guid     Id { get; set; }
        public Guid     BrandId { get; set; }
        public string   Name { get; set; }
        public bool     IsMain { get; set; }
        public string   CurrencyCode { get; set; }

        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }

        public Brand Brand { get; set; }
        public ICollection<WalletTemplateProduct> WalletTemplateProducts { get; set; } 
    }
}