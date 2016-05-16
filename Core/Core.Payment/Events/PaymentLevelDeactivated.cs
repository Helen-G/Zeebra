using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentLevelDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }
        public string Remarks { get; set; }

        public PaymentLevelDeactivated()
        {            
        }

        public PaymentLevelDeactivated(PaymentLevel paymentLevel)
        {
            Id = paymentLevel.Id;
            Code = paymentLevel.Code;
            Name = paymentLevel.Name;
            DeactivatedBy = paymentLevel.DeactivatedBy;
            DeactivatedDate = paymentLevel.DateDeactivated.Value;
        }
    }
}
