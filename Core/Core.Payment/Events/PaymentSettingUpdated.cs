using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentSettingUpdated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string VipLevel { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }

        public PaymentSettingUpdated()
        {
        }

        public PaymentSettingUpdated(PaymentSettings setting)
        {
            Id = setting.Id;
            UpdatedBy = setting.UpdatedBy;
            UpdatedDate = setting.UpdatedDate.GetValueOrDefault();
            VipLevel = setting.VipLevel;
            CurrencyCode = setting.CurrencyCode;
            BrandId = setting.BrandId;
        }
    }
}
