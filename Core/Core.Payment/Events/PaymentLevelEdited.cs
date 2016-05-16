using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentLevelEdited : DomainEventBase
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }

        public PaymentLevelEdited()
        {
        }

        public PaymentLevelEdited(PaymentLevel paymentLevel)
        {
            Code = paymentLevel.Code;
            Name = paymentLevel.Name;
            UpdatedBy = paymentLevel.UpdatedBy;
            UpdatedDate = paymentLevel.DateUpdated.GetValueOrDefault();
        }
    }
}
