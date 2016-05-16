using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class WalletTemplateCreated : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public WalletTemplateDto[] WalletTemplates { get; set; }
    }

    public class WalletTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public string CurrencyCode { get; set; }

        public Guid[] ProductIds { get; set; }
    }
}