using System;
using AFT.RegoV2.Core.Common.Interfaces.Brand;

namespace AFT.RegoV2.Core.Brand.ApplicationServices.Data
{
    public class AssignBrandCurrencyData : IAssignBrandCurrencyData
    {
        public Guid Brand { get; set; }
        public string[] Currencies { get; set; }
        public string DefaultCurrency { get; set; }
        public string BaseCurrency { get; set; }
    }
}
