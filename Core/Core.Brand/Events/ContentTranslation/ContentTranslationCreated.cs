using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events.ContentTranslation
{
    public class ContentTranslationCreated : DomainEventBase
    {
        public ContentTranslationCreated() { }

        public ContentTranslationCreated(Data.ContentTranslation contentTranslation)
        {
            DateCreated = contentTranslation.Created;
            CreatedBy = contentTranslation.CreatedBy;
            Name = contentTranslation.Name;
            Source = contentTranslation.Source;
            Status = contentTranslation.Status;
            Remark = contentTranslation.Remark;
            Language = contentTranslation.Language;
        }

        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public TranslationStatus Status { get; set; }
        public string Remark { get; set; }
        public string Language { get; set; }
    }
}
