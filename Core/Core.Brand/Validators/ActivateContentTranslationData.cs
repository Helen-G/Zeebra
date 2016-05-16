using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class ActivateContentTranslationData : IActivateContentTranslationData
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}