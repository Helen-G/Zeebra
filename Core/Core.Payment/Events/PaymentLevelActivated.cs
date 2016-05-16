using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentLevelActivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset ActivatedDate { get; set; }
        public string Remarks { get; set; }

        public PaymentLevelActivated()
        {            
        }

        public PaymentLevelActivated(PaymentLevel paymentLevel)
        {
            Id = paymentLevel.Id;
            Code = paymentLevel.Code;
            Name = paymentLevel.Name;
            ActivatedBy = paymentLevel.ActivatedBy;
            ActivatedDate = paymentLevel.DateActivated.Value;
        }
    }
}
