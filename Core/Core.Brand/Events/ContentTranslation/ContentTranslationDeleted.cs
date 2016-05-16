using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events.ContentTranslation
{
    public class ContentTranslationDeleted : DomainEventBase
    {
        public ContentTranslationDeleted() { }

        public ContentTranslationDeleted(Data.ContentTranslation contentTranslation)
        {
            Name = contentTranslation.Name;
            Source = contentTranslation.Source;
        }

        public string Name { get; set; }
        public string Source { get; set; }
    }
}
