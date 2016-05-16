using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentSettingDeactivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedDate { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }

        public PaymentSettingDeactivated()
        {
        }

        public PaymentSettingDeactivated(PaymentSettings setting)
        {
            Id = setting.Id;
            DeactivatedBy = setting.UpdatedBy;
            DeactivatedDate = setting.DisabledDate.GetValueOrDefault();
            VipLevel = setting.VipLevel;
            CurrencyCode = setting.CurrencyCode;
            BrandId = setting.BrandId;
        }
    }
}
