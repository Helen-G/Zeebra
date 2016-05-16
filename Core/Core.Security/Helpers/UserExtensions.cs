using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Security.Helpers
{
    public static class UserExtensions
    {
        public static void SetLicensees(this User user, IEnumerable<Guid> licensees)
        {
            if (licensees == null) return;

            user.Licensees.Clear();

            licensees.Select(licensee => new UserLicenseeId { Id = licensee, UserId = user.Id })
                .ForEach(licenseeId =>
                {
                    user.Licensees.Add(licenseeId);
                });
        }

        public static void SetAllowedBrands(this User user, IEnumerable<Guid> allowedBrands)
        {
            if (allowedBrands == null) return;

            user.AllowedBrands.Clear();

            allowedBrands.Select(allowedBrandId => new BrandId { Id = allowedBrandId, UserId = user.Id })
                .ForEach(brandId => user.AllowedBrands.Add(brandId));
        }

        public static void SetCurrencies(this User user, IEnumerable<string> currencies)
        {
            if (currencies == null) return;

            user.Currencies.Clear();

            currencies.Select(currencyCode => new CurrencyCode { Currency = currencyCode, UserId = user.Id })
                .ForEach(currency =>
                {
                    user.Currencies.Add(currency);
                });
        }

        public static void AddAllowedBrand(this User user, Guid brandId)
        {
            if (user.AllowedBrands.Any(b => b.Id == brandId)) return;

            var allowedBrand = new BrandId { Id = brandId, UserId = user.Id };

            user.AllowedBrands.Add(allowedBrand);
        }
    }
}
