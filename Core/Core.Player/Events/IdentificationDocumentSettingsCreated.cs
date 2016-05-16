using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Core.Player.Events
{
    public class IdentificationDocumentSettingsCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public Guid LicenseeId { get; set; }
        public Guid BrandId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public Common.Data.Payment.PaymentMethod? PaymentMethod { get; set; }
        public bool IdFront { get; set; }
        public bool IdBack { get; set; }
        public bool CreditCardFront { get; set; }
        public bool CreditCardBack { get; set; }
        public bool POA { get; set; }
        public bool DCF { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? UpdatedOn { get; set; }

        public IdentificationDocumentSettingsCreated()
        {
        } // default constructor is required for publishing event to MQ

        public IdentificationDocumentSettingsCreated(IdentificationDocumentSettings setting)
        {
            AutoMapper.Mapper.DynamicMap(setting, this);
        }
    }
}