using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentSettingCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string VipLevel { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }

        public PaymentSettingCreated()
        {
        }

        public PaymentSettingCreated(PaymentSettings setting)
        {
            Id = setting.Id;
            CreatedBy = setting.CreatedBy;
            CreatedDate = setting.CreatedDate;
            VipLevel = setting.VipLevel;
            CurrencyCode = setting.CurrencyCode;
            BrandId = setting.BrandId;
        }
    }
}
