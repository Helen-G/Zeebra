using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class LanguageStatusChanged : DomainEventBase
    {
        public LanguageStatusChanged() { }

        public LanguageStatusChanged(Culture culture)
        {
            Code = culture.Code;
            Status = culture.Status;
            DateStatusChanged = culture.Status == CultureStatus.Active
                ? culture.DateActivated.Value
                : culture.DateDeactivated.Value;
            StatusChangedBy = culture.Status == CultureStatus.Active
                ? culture.ActivatedBy
                : culture.DeactivatedBy;
        }

        public string Code { get; set; }
        public CultureStatus Status { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTimeOffset DateStatusChanged { get; set; }
        public string Remarks { get; set; }
    }
}