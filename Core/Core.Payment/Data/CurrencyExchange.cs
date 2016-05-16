using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class CurrencyExchange
    {
        [Key, ForeignKey("Brand"), Column(Order = 0)]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Key, ForeignKey("CurrencyTo"), Column(Order = 1)]
        public string CurrencyToCode { get; set; }
        public Currency CurrencyTo { get; set; }

        [Required]
        public decimal CurrentRate { get; set; }

        public decimal? PreviousRate { get; set; }

        //commented for R.01
        //[DefaultValue(false)]
        //public bool IsLiveRate { get; set; }

        [DefaultValue(false)]
        public bool IsBaseCurrency { get; set; }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset? DateUpdated { get; set; }
    }
}