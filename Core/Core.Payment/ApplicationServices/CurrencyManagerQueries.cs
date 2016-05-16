using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class CurrencyManagerQueries : IApplicationService
    {
        private readonly BrandQueries _brandQueries;

        public CurrencyManagerQueries(BrandQueries brandQueries)
        {
            _brandQueries = brandQueries;
        }

        public IEnumerable<string> GetCurrencyCodes(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var currencyCodes = brand.BrandCurrencies.Select(c => c.CurrencyCode);

            return currencyCodes;
        }

        public object GetAssignData(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var licensee = _brandQueries.GetLicensees()
                .Single(l => l.Brands.Any(b => b.Id == brandId));
            var allowedCurrencies = licensee.Currencies.OrderBy(c => c.Name);
            var assignedCurrencies = brand.BrandCurrencies.Select(x => x.Currency).ToArray();
            var availableCurrencies = allowedCurrencies.Except(assignedCurrencies);

            return new
            {
                AvailableCurrencies = availableCurrencies.Select(x => x.Code).ToList(),
                AssignedCurrencies = assignedCurrencies.Select(x => x.Code).ToList(),
                brand.DefaultCurrency,
                brand.BaseCurrency,
                IsActive = brand.Status == BrandStatus.Active
            };
        }
    }
}
