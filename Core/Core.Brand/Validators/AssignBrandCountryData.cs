using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AssignBrandCountryData : IAssignBrandCountryData
    {
        public Guid Brand { get; set; }
        public string[] Countries { get; set; }
    }
}