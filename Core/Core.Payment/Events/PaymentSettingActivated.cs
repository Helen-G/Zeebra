using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.Events
{
    public class PaymentSettingActivated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string ActivatedBy { get; set; }
        public DateTimeOffset ActivatedDate { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
        public string Remarks { get; set; }

        public PaymentSettingActivated()
        {
        }

        public PaymentSettingActivated(PaymentSettings setting)
        {
            Id = setting.Id;
            ActivatedBy = setting.UpdatedBy;
            ActivatedDate = setting.EnabledDate.GetValueOrDefault();
            VipLevel = setting.VipLevel;
            CurrencyCode = setting.CurrencyCode;
            BrandId = setting.BrandId;
        }
    }
}
