using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class DeactivateContentTranslationData : IDeactivateContentTranslationData
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }
    }
}