using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Brand
{
    public class WalletTemplateUpdated : DomainEventBase
    {
        public Guid                BrandId { get; set; }
        public Guid[]              RemovedWalletTemplateIds { get; set; }
        public WalletTemplateDto[] RemainedWalletTemplates { get; set; }
        public WalletTemplateDto[] NewWalletTemplates { get; set; }
    }
}