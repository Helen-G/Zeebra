using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class TransferFundSettingsDeactivated : DomainEventBase
    {
        public TransferFundSettingsDeactivated() { } // default constructor is required for publishing event to MQ

        public TransferFundSettingsDeactivated(TransferSettings settings)
        {
            TransferSettingsId = settings.Id;
            DisabledBy = settings.DisabledBy;
            Deactivated = settings.DisabledDate;
        }

        public Guid TransferSettingsId { get; set; }
        public string DisabledBy { get; set; }
        public DateTimeOffset? Deactivated { get; set; }
        public string Remarks { get; set; }
    }
}
