using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class PaymentSettingsId
    {
        private readonly Guid _id;

        public PaymentSettingsId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PaymentSettingsId id)
        {
            return id._id;
        }

        public static implicit operator PaymentSettingsId(Guid id)
        {
            return new PaymentSettingsId(id);
        }
    }

    public class PaymentSettings
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public Brand Brand { get; protected set; }

        public PaymentType PaymentType { get; set; }

        [Required]
        public string VipLevel { get; set; }

        [Required, StringLength(3)]
        public string CurrencyCode { get; set; }

        public PaymentGateway PaymentGateway { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinAmountPerTransaction { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAmountPerTransaction { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAmountPerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerWeek { get; set; }

        [Range(0, int.MaxValue)]
        public int MaxTransactionPerMonth { get; set; }

        public bool Enabled { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string EnabledBy { get; set; }
        public DateTime? EnabledDate { get; set; }
        public string DisabledBy { get; set; }
        public DateTime? DisabledDate { get; set; }
    }
}