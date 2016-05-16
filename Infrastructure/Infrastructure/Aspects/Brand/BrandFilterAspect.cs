using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Aspects.Base;
using AFT.RegoV2.Shared.Constants;

namespace AFT.RegoV2.Infrastructure.Aspects
{
    public class BrandFilterAspect : FilterAspectBase,
        IFilter<IEnumerable<Brand>>,
        IFilter<IEnumerable<Licensee>>,
        IFilter<IEnumerable<VipLevel>>,
        IFilter<IEnumerable<BrandCountry>>,
        IFilter<IEnumerable<BrandCulture>>
    {
        private readonly IBrandRepository _repository;
        private readonly ISecurityRepository _securityRepository;

        public BrandFilterAspect(
            IBrandRepository repository,
            ISecurityRepository securityRepository,
            ISecurityProvider securityProvider,
            IUserInfoProvider userInfoProvider)
            :base(securityProvider, userInfoProvider)
        {
            if (repository == null) 
                throw new ArgumentNullException("repository");
            if (securityRepository == null) 
                throw new ArgumentNullException("securityRepository");

            _repository = repository;
            _securityRepository = securityRepository;
        }
        public IEnumerable<Brand> Filter(
            IEnumerable<Brand> brands,
            Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(u => u.AllowedBrands)
                .Include(u => u.Licensees)
                .Include(u => u.Role)
                .SingleOrDefault(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return brands.AsQueryable();

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

        public IEnumerable<Licensee> Filter(IEnumerable<Licensee> licensees, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .SingleOrDefault(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return licensees;

            return licensees.Where(l => user.Licensees.Any(x => l.Id == x.Id) && l.Status == LicenseeStatus.Active).AsQueryable();
        }

        public IEnumerable<VipLevel> Filter(IEnumerable<VipLevel> vipLevels, Guid userId)
        {
            if (userId == Guid.Empty)
                return vipLevels;
            
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
                return vipLevels.AsQueryable().Include(x => x.Brand).Where(x => allowedBrandIds.Contains(x.Brand.Id));
            }

            if (userRoleId == RoleIds.LicenseeId)
            {
                var licensee = _repository.Licensees.Include(x => x.Brands).ToList().Single(x => x.Id == user.Licensees.First().Id);
                if (licensee == null)
                    return null;

                var allowedLicenseeBrandsIds = licensee.Brands.Select(x => x.Id);

                return vipLevels.AsQueryable().Include(x => x.Brand).Where(x => allowedLicenseeBrandsIds.Contains(x.Brand.Id));
            }

            return null;
        }

        public IEnumerable<BrandCountry> Filter(IEnumerable<BrandCountry> countries, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .SingleOrDefault(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return countries;

            var userCountries = _repository.Licensees
                .Include(l => l.Countries)
                .Where(l => user.Licensees.Any(ul => ul.Id == l.Id))
                .SelectMany(l => l.Countries)
                .Union(
                _repository.Brands
                .Where(b => user.AllowedBrands.Any(ab => ab.Id == b.Id))
                .SelectMany(b => b.BrandCountries)
                .Select(bc => bc.Country));

            return countries.Where(c => userCountries.Any(uc => uc.Code == c.CountryCode));
        }

        public IEnumerable<BrandCulture> Filter(IEnumerable<BrandCulture> cultures, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.Licensees)
                .SingleOrDefault(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return cultures;

            var userCountries = _repository.Licensees
                .Include(l => l.Cultures)
                .Where(l => user.Licensees.Any(ul => ul.Id == l.Id))
                .SelectMany(l => l.Cultures)
                .Union(
                _repository.Brands
                .Where(b => user.AllowedBrands.Any(ab => ab.Id == b.Id))
                .SelectMany(b => b.BrandCultures)
                .Select(bc => bc.Culture));

            return cultures.Where(c => userCountries.Any(uc => uc.Code == c.CultureCode));
        }

    }
}
