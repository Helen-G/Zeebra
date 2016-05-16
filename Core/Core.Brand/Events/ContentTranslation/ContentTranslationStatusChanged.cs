using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events.ContentTranslation
{
    public class ContentTranslationStatusChanged : DomainEventBase
    {
        public ContentTranslationStatusChanged() { }

        public ContentTranslationStatusChanged(Data.ContentTranslation contentTranslation)
        {
            Name = contentTranslation.Name;
            Status = contentTranslation.Status;

            DateStatusChanged = contentTranslation.Status == TranslationStatus.Enabled ? contentTranslation.Activated.Value : contentTranslation.Deactivated.Value;
            StatusChangedBy = contentTranslation.Status == TranslationStatus.Enabled ? contentTranslation.ActivatedBy : contentTranslation.DeactivatedBy;
        }

        public string StatusChangedBy { get; set; }
        public DateTimeOffset DateStatusChanged { get; set; }

        public string Name { get; set; }

        public TranslationStatus Status { get; set; }


    }
}
