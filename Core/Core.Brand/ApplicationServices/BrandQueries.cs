using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Constants;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public class BrandQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IBrandRepository _repository;
        private readonly ISecurityRepository _securityRepository;

        public BrandQueries(
            IBrandRepository repository,
            ISecurityRepository securityRepository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");
            if (securityRepository == null)
                throw new ArgumentNullException("securityRepository");

            _repository = repository;
            _securityRepository = securityRepository;
        }

        #region licensee
        [Filtered]
        [Permission(Permissions.View, Module = Modules.LicenseeManager)]
        public IQueryable<Licensee> GetAllLicensees()
        {
            return GetLicensees();
        }

        public IQueryable<Licensee> GetLicensees()
        {
            return _repository.Licensees
                .Include(x => x.Brands)
                .Include(x => x.Currencies)
                .Include(x => x.Cultures)
                .Include(x => x.Countries)
                .Include(x => x.Products)
                .Include(x => x.Contracts)
                .Include(l => l.Brands.Select(b => b.BrandCurrencies));
        }

        public Licensee GetLicensee(Guid licenseeId)
        {
            return GetLicensees().SingleOrDefault(x => x.Id == licenseeId);
        }

        public IEnumerable<Contract> GetLicenseeContracts(Guid licenseeId)
        {
            var licensee = GetLicensee(licenseeId);

            return licensee == null ? null : licensee.Contracts;
        }

        public bool CanActivateLicensee(Licensee licensee)
        {
            return (licensee.Status == LicenseeStatus.Inactive || licensee.Status == LicenseeStatus.Deactivated) &&
                   (!licensee.ContractEnd.HasValue || licensee.ContractEnd > DateTimeOffset.UtcNow);
        }

        public bool CanRenewLicenseeContract(Licensee licensee)
        {
            return licensee.ContractEnd.HasValue &&
                licensee.ContractEnd.Value < DateTimeOffset.UtcNow &&
                licensee.Status == LicenseeStatus.Active;
        }

        public IQueryable<Licensee> GetFilteredLicensees(IEnumerable<Licensee> licensees, Guid userId)
        {
            var user = _securityRepository.Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Single(u => u.Id == userId);

            return user.Role.Id == RoleIds.SuperAdminId
                ? licensees.AsQueryable()
                : licensees.Where(l => user.Licensees.Any(x => l.Id == x.Id) && l.Status == LicenseeStatus.Active).AsQueryable();
        }

        #endregion licensee

        #region brand

        [Filtered]
        [Permission(Permissions.View, Module = Modules.BrandManager)]
        public IQueryable<Brand.Data.Brand> GetAllBrands()
        {
            return GetBrands();
        }

        public IQueryable<Brand.Data.Brand> GetBrands()
        {
            return _repository.Brands
                .Include(x => x.BrandCountries.Select(y => y.Country))
                .Include(x => x.BrandCountries.Select(y => y.Brand))
                .Include(x => x.BrandCultures.Select(y => y.Culture))
                .Include(x => x.BrandCurrencies.Select(y => y.Currency))
                .Include(x => x.Licensee)
                .Include(x => x.Licensee.Brands)
                .Include(x => x.Licensee.Currencies)
                .Include(x => x.Licensee.Cultures)
                .Include(x => x.Licensee.Countries)
                .Include(x => x.Licensee.Products)
                .Include(x => x.Licensee.Contracts)
                .Include(x => x.BrandCurrencies)
                .Include(x => x.VipLevels)
                .Include(x => x.WalletTemplates)
                .Include(x => x.Products.Select(y => y.Brand));
        }

        public Brand.Data.Brand GetBrandForActivation(Guid brandId)
        {
            var brand = _repository.Brands
                .Include(b => b.BrandCountries.Select(x => x.Country))
                .Include(b => b.BrandCultures.Select(x => x.Culture))
                .Include(b => b.BrandCurrencies.Select(x => x.Currency))
                .Include(b => b.VipLevels)
                .Include(b => b.WalletTemplates)
                .Include(b => b.Licensee.Brands)
                .Include(b => b.Products)
                .Include(b => b.DefaultVipLevel)
                .Single(b => b.Id == brandId);

            return brand;
        }

        public Brand.Data.Brand GetBrandOrNull(Guid brandId)
        {
            return GetBrands().SingleOrDefault(x => x.Id == brandId);
        }

        public PlayerActivationMethod GetPlayerActivationMethod(Guid brandId)
        {
            return _repository.Brands.Find(brandId).PlayerActivationMethod;
        }

        public async Task<Core.Brand.Data.Brand> GetBrandOrNullAsync(Guid brandId)
        {
            return await GetBrands().SingleOrDefaultAsync(x => x.Id == brandId);
        }

        public Brand.Data.Brand GetBrand(Guid brandId)
        {
            var result = GetBrandOrNull(brandId);
            if (result == null)
                throw new RegoException(string.Format("Unable to find brand with Id '{0}'", brandId));

            return result;
        }

        public IEnumerable<Brand.Data.Brand> GetBrandsByLicensee(Guid licenseeId)
        {
            var licensee = GetLicensee(licenseeId);

            return licensee == null ? null : licensee.Brands;
        }

        public bool IsBrandActive(Guid brandId)
        {
            return _repository.Brands.Any(x => x.Id == brandId && x.Status == BrandStatus.Active);
        }

        public bool DoesBrandExist(Guid brandId)
        {
            return _repository.Brands.Find(brandId) != null;
        }

        public bool BrandHasCurrency(Guid brandId, string currencyCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCurrencies.Select(x => x.Currency))
                .Any(x => x.Id == brandId && x.BrandCurrencies.Any(y => y.CurrencyCode == currencyCode));
        }

        public bool BrandHasCountry(Guid brandId, string countryCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCountries.Select(x => x.Country))
                .Any(x => x.Id == brandId && x.BrandCountries.Any(y => y.CountryCode == countryCode));
        }

        public bool BrandHasCulture(Guid brandId, string cultureCode)
        {
            return _repository.Brands
                .Include(b => b.BrandCultures.Select(x => x.Culture))
                .Any(x => x.Id == brandId && x.BrandCultures.Any(y => y.CultureCode == cultureCode));
        }

        public bool IsCountryAssignedToAnyBrand(string countryCode)
        {

            return GetBrands().Any(x => x.BrandCountries.Any(y => y.CountryCode == countryCode));
        }

        public IQueryable<Brand.Data.Brand> GetFilteredBrands(IEnumerable<Brand.Data.Brand> brands, Guid userId)
        {
            var user = _securityRepository.Users
                .Include(u => u.AllowedBrands)
                .Include(u => u.Licensees)
                .Include(u => u.Role)
                .Single(u => u.Id == userId);

            if (user.Role.Id == RoleIds.MultipleBrandManagerId || user.Role.Id == RoleIds.SingleBrandManagerId)
            {
                var allowedBrandsIds = user.AllowedBrands.Select(x => x.Id);
                return brands.Where(x => allowedBrandsIds.Contains(x.Id)).AsQueryable();
            }

            if (user.Role.Id == RoleIds.LicenseeId)
            {
                var id = user.Licensees.Select(x => x.Id).First();
                var licensee = _repository.Licensees.Include(x => x.Brands).First(x => x.Id == id);
                return brands.Intersect(licensee.Brands.Where(b => user.AllowedBrands.Any(ab => ab.Id == b.Id))).AsQueryable();
            }

            var filtered = user.AllowedBrands.Any()
                ? brands.Where(brand => user.AllowedBrands.Select(b => b.Id).Contains(brand.Id)).AsQueryable()
                : brands.AsQueryable();

            return filtered;
        }

        #endregion brand

        #region culture

        public IQueryable<Culture> GetCultures()
        {
            return _repository.Cultures.AsNoTracking();
        }

        public Culture GetCulture(string cultureCode)
        {
            return GetCultures().SingleOrDefault(c => c.Code == cultureCode);
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.SupportedLanguages)]
        public IEnumerable<BrandCulture> GetAllBrandCultures()
        {
            var brands = GetBrands().ToDictionary(b => b.Id, b => b);
            var brandCultures = GetBrands().SelectMany(b => b.BrandCultures);

            brandCultures.ForEach(bc =>
            {
                bc.Brand = brands[bc.BrandId];
            });

            return brandCultures;
        }

        public IEnumerable<BrandCulture> GetBrandCultures(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null ? null : brand.BrandCultures;
        }

        public IEnumerable<Culture> GetCulturesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCultures.Select(x => x.Culture);
        }

        public IEnumerable<Culture> GetAllowedCulturesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.Licensee.Cultures;
        }

        public IEnumerable<Culture> GetActiveCultures()
        {
            return GetCultures().Where(x => x.Status == CultureStatus.Active);
        }

        #endregion culture

        #region country

        [Permission(Permissions.View, Module = Modules.CountryManager)]
        public IQueryable<Country> GetAllCountries()
        {
            return GetCountries();
        }

        public IQueryable<Country> GetCountries()
        {
            return _repository.Countries.AsNoTracking();
        }

        public Country GetCountry(string countryCode)
        {
            return GetCountries().SingleOrDefault(x => x.Code == countryCode);
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.SupportedCountries)]
        public IEnumerable<BrandCountry> GetAllBrandCountries()
        {
            var countries = GetCountries().ToDictionary(c => c.Code, c => c);
            var brands = GetBrands().ToDictionary(b => b.Id, b => b);
            var brandCountries = GetBrands().SelectMany(b => b.BrandCountries);
            brandCountries.ForEach(bc =>
            {
                bc.Brand = brands[bc.BrandId];
                bc.Country = countries[bc.CountryCode];
            });

            return brandCountries;
        }

        public IEnumerable<BrandCountry> GetBrandCountries(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null ? null : brand.BrandCountries;
        }

        public IEnumerable<Country> GetCountriesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCountries.Select(x => x.Country);
        }

        public IEnumerable<Country> GetAllowedCountriesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.Licensee.Countries;
        }

        #endregion country

        #region currency

        public IQueryable<Currency> GetCurrencies()
        {
            return _repository.Currencies.AsNoTracking();
        }

        public Currency GetCurrency(string currencyCode)
        {
            return GetCurrencies().SingleOrDefault(x => x.Code == currencyCode);
        }

        public IQueryable<BrandCurrency> GetBrandsCurrencies(Guid? brandId)
        {
            return brandId.HasValue
                ? GetBrandOrNull(brandId.Value).BrandCurrencies.AsQueryable()
                : GetBrands().SelectMany(b => b.BrandCurrencies);
        }

        public IEnumerable<BrandCurrency> GetBrandCurrencies(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null ? null : brand.BrandCurrencies;
        }

        public IEnumerable<Currency> GetCurrenciesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.BrandCurrencies.Select(x => x.Currency);
        }

        public IEnumerable<Currency> GetAllowedCurrenciesByBrand(Guid brandId)
        {
            var brand = GetBrandOrNull(brandId);

            return brand == null
                ? null
                : brand.Licensee.Currencies;
        }

        public Guid? GetDefaultPaymentLevelId(Guid brandId, string currencyCode)
        {
            var brand = _repository
                .Brands
                .Include(o => o.BrandCurrencies)
                .Single(x => x.Id == brandId);

            var brandCurrency = brand.BrandCurrencies
                .Single(o => o.CurrencyCode == currencyCode);

            var defaultPaymentLevelId = brandCurrency.DefaultPaymentLevelId;

            return defaultPaymentLevelId;
        }

        #endregion currency

        #region vip

        [Permission(Permissions.View, Module = Modules.VipLevelManager)]
        public IQueryable<VipLevel> GetVipLevels()
        {
            return _repository.VipLevels
                .Include(x => x.Brand.Licensee)
                .Include(x => x.VipLevelLimits.Select(y => y.Currency))
                .AsNoTracking();
        }

        [Filtered]
        [Permission(Permissions.View, Module = Modules.VipLevelManager)]
        public IQueryable<VipLevel> GetAllVipLevels()
        {
            return GetVipLevels();
        }

        public VipLevel GetVipLevel(Guid vipLevelId)
        {
            return GetVipLevels().SingleOrDefault(x => x.Id == vipLevelId);
        }

        public IQueryable<VipLevel> GetFilteredVipLevels(IQueryable<VipLevel> vipLevels, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .Include(x => x.AllowedBrands)
                .Single(x => x.Id == userId);

            var userRoleId = user.Role.Id;

            if (userRoleId == RoleIds.SuperAdminId)
                return vipLevels;

            if (userRoleId == RoleIds.SingleBrandManagerId || userRoleId == RoleIds.MultipleBrandManagerId)
            {
                var allowedBrandIds = user.AllowedBrands.Select(x => x.Id);

                return vipLevels.Where(x => allowedBrandIds.Contains(x.Brand.Id));
            }

            if (userRoleId == RoleIds.LicenseeId)
            {
                var licensee = GetLicensee(user.Licensees.First().Id);

                if (licensee == null)
                    return null;

                var allowedLicenseeBrandsIds = licensee.Brands.Select(x => x.Id);

                return vipLevels.Where(x => allowedLicenseeBrandsIds.Contains(x.Brand.Id));
            }

            return null;
        }

        public VipLevelGameProviderBetLimit GetVipLevelGameProviderBetLimit(Guid vipLevelId, Guid gameProviderId, string currency)
        {
            return GetVipLevel(vipLevelId)
                .VipLevelLimits
                .SingleOrDefault(y =>
                    y.GameProviderId == gameProviderId &&
                    y.Currency.Code == currency);
        }

        public VipLevelViewModel GetVipLevelViewModel(Guid vipLevelId)
        {
            var entity = GetVipLevel(vipLevelId);
            if (entity == null)
                return null;

            var vipLevel = new VipLevelViewModel
            {
                Id = entity.Id,
                Brand = entity.Brand.Id,
                Code = entity.Code,
                Name = entity.Name,
                Rank = entity.Rank,
                Description = entity.Description,
                Color = entity.ColorCode,
                Limits = entity
                    .VipLevelLimits
                    .Select(x => new VipLevelLimitViewModel
                    {
                        Id = x.Id,
                        CurrencyCode = x.Currency.Code,
                        GameProviderId = x.GameProviderId,
                        BetLimitId = x.BetLimitId
                    }).ToList(),
            };

            return vipLevel;
        }

        #endregion vip

        #region products

        public IEnumerable<LicenseeProduct> GetAllowedProductsByBrand(Guid brandId)
        {
            return GetBrandOrNull(brandId).Licensee.Products;
        }

        [Permission(Permissions.View, Module = Modules.SupportedProducts)]
        public IEnumerable<BrandProduct> GetAllowedProducts(Guid userId, Guid? brandId = null)
        {
            var queryable = GetBrands();

            if (brandId.HasValue)
            {
                queryable = queryable.Where(x => x.Id == brandId);
            }

            var allowedBrands = GetFilteredBrands(queryable, userId);

            return allowedBrands.SelectMany(x => x.Products);
        }

        #endregion products

        #region wallet

        public WalletTemplate GetWalletTemplate(Guid id)
        {
            var walletTemplate = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .FirstOrDefault(x => x.Id == id);
            return walletTemplate;
        }

        public IEnumerable<WalletTemplate> GetWalletTemplates(Guid brandId)
        {
            var walletTemplates = _repository
                .Brands
                .Include(x => x.WalletTemplates.Select(wt => wt.WalletTemplateProducts))
                .Single(x => x.Id == brandId)
                .WalletTemplates;

            return walletTemplates;
        }

        [Permission(Permissions.View, Module = Modules.WalletManager)]
        public IQueryable<WalletListDTO> GetWalletTemplates()
        {
            var walletTemplates = _repository
                .WalletTemplates
                .Include(x => x.Brand)
                .Include(x => x.Brand.Licensee)
                .ToList();

            var grouped = walletTemplates
                .GroupBy(x => x.Brand.Id)
                .SelectMany(x => x.OrderBy(w => w.DateUpdated))
                .ToList();

            var listOfDtos = new List<WalletListDTO>();

            foreach (var walletTemplate in grouped)
            {
                if (!walletTemplate.IsMain)
                    continue;

                listOfDtos.Add(new WalletListDTO()
                {
                    Brand = walletTemplate.Brand.Id,
                    BrandName = walletTemplate.Brand.Name,
                    LicenseeId = walletTemplate.Brand.Licensee.Id,
                    LicenseeName = walletTemplate.Brand.Licensee.Name,
                    CreatedBy = walletTemplate.CreatedBy == Guid.Empty ? "" : _securityRepository.Users.First(u => u.Id == walletTemplate.CreatedBy).FirstName,
                    DateCreated = walletTemplate.DateCreated,
                    DateUpdated = walletTemplate.DateUpdated,
                    UpdatedBy = walletTemplate.UpdatedBy == Guid.Empty ? "" : _securityRepository.Users.First(u => u.Id == walletTemplate.UpdatedBy).FirstName
                });
            }

            return listOfDtos.AsQueryable();
        }

        #endregion wallet

        #region fraud

        public IEnumerable<RiskLevel> GetRiskLevels(Guid brandId)
        {
            return _repository.RiskLevels.Where(x => x.BrandId == brandId);
        }

        #endregion fraud
    }
}