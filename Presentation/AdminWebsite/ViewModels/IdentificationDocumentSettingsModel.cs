using System;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class IdentificationDocumentSettingsModel
    {
        public Guid? Id { get; set; }
        public Guid LicenseeId { get; set; }
        public Guid BrandId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public bool IdFront { get; set; }
        public bool IdBack { get; set; }
        public bool CreditCardFront { get; set; }
        public bool CreditCardBack { get; set; }
        public bool POA { get; set; }
        public bool DCF { get; set; }
        public string Remark { get; set; }
    }
}
