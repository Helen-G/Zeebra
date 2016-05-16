using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class TransferFundSettingsCreated : DomainEventBase
    {
        public TransferFundSettingsCreated() { } // default constructor is required for publishing event to MQ

        public TransferFundSettingsCreated(TransferSettings settings)
        {
            TransferSettingsId = settings.Id;
            CreatedBy = settings.CreatedBy;
            Created = settings.CreatedDate;
        }

        public Guid TransferSettingsId { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
