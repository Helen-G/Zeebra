using System;
using AFT.RegoV2.Core.Brand.Validators.ContentTranslations.Base;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators.ContentTranslations
{
    public class EditContentTranslationData : ContentTranslationBase, IEditContentTranslationData
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public string Translation { get; set; }
        public string Remark { get; set; }
    }
}
