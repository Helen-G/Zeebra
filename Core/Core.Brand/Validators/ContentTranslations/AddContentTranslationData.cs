using AFT.RegoV2.Core.Brand.Validators.ContentTranslations.Base;

namespace AFT.RegoV2.Core.Brand.Validators.ContentTranslations
{
    public class AddContentTranslationData : ContentTranslationBase
    {
        public string Language { get; set; }
        public string Translation { get; set; }
    }
}
