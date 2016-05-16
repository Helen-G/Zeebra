using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events.ContentTranslation
{
    public class ContentTranslationUpdated : DomainEventBase
    {
        public ContentTranslationUpdated() { }

        public ContentTranslationUpdated(Data.ContentTranslation contentTranslation)
        {
            DateUpdated = contentTranslation.Updated.Value;
            UpdatedBy = contentTranslation.UpdatedBy;
            Name = contentTranslation.Name;
            Source = contentTranslation.Source;
            Status = contentTranslation.Status;
            Remark = contentTranslation.Remark;
            Language = contentTranslation.Language;
        }

        public string UpdatedBy { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public TranslationStatus Status { get; set; }
        public string Remark { get; set; }
        public string Language { get; set; }
    }
}
