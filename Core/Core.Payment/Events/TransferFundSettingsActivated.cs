using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class TransferFundSettingsActivated : DomainEventBase
    {
        public TransferFundSettingsActivated() { } // default constructor is required for publishing event to MQ

        public TransferFundSettingsActivated(TransferSettings settings)
        {
            TransferSettingsId = settings.Id;
            EnabledBy = settings.EnabledBy;
            Activated = settings.EnabledDate;
        }

        public Guid TransferSettingsId { get; set; }
        public string EnabledBy { get; set; }
        public DateTimeOffset? Activated { get; set; }
        public string Remarks { get; set; }
    }
}
