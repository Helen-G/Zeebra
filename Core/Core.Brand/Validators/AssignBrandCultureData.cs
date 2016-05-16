using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandCultureData : IAssignBrandCultureData
    {
        public Guid Brand { get; set; }
        public string[] Cultures { get; set; }
        public string DefaultCulture { get; set; }
    }
}