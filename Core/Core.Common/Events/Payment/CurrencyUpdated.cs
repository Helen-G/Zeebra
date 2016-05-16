using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Payment
{
    public class CurrencyUpdated : DomainEventBase
    {
        public string OldCode { get; set; }
        public string OldName { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
    }
}
