using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class TransferFundSettingsUpdated : DomainEventBase
    {
        public TransferFundSettingsUpdated() { } // default constructor is required for publishing event to MQ

        public TransferFundSettingsUpdated(TransferSettings settings)
        {
            TransferSettingsId = settings.Id;
            UpdatedBy = settings.UpdatedBy;
            Updated = settings.UpdatedDate;
        }

        public Guid TransferSettingsId { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
