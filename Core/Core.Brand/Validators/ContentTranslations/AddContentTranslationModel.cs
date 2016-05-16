using System.Collections.Generic;
using AFT.RegoV2.Core.Brand.Validators.ContentTranslations.Base;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators.ContentTranslations
{
    public class AddContentTranslationModel : ContentTranslationBase, IAddContentTranslationModel
    {
        public IList<string> Languages { get; set; }
        public IList<AddContentTranslationData> Translations { get; set; }
    }
}