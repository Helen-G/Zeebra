using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Payment
{
    public class CurrencyExchangeUpdated : DomainEventBase
    {
        public Guid BrandId { get; set; }
        public string CurrencyToCode { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal PreviousRate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}
