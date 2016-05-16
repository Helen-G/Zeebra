using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Payment
{
    public class CurrencyStatusChanged : DomainEventBase
    {
        public string Code { get; set; }
        public CurrencyStatus Status { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTimeOffset DateStatusChanged { get; set; }
        public string Remarks { get; set; }
    }
}
