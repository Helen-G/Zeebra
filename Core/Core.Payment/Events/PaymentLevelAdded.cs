using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentLevelAdded : DomainEventBase
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public PaymentLevelAdded()
        {
        }

        public PaymentLevelAdded(PaymentLevel paymentLevel)
        {
            Code = paymentLevel.Code;
            Name = paymentLevel.Name;
            CreatedBy = paymentLevel.CreatedBy;
            CreatedDate = paymentLevel.DateCreated;
        }
    }
}
