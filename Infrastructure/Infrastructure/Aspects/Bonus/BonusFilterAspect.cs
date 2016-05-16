using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Aspects.Base;
using AFT.RegoV2.Shared.Constants;

namespace AFT.RegoV2.Infrastructure.Aspects
{
    public class BonusFilterAspect : FilterAspectBase,
        IFilter<IQueryable<Bonus>>,
        IFilter<IQueryable<Template>>,
        IFilter<IQueryable<BonusRedemption>>
    {
        private readonly ISecurityRepository _securityRepository;

        public BonusFilterAspect(
            ISecurityRepository securityRepository,
            ISecurityProvider securityProvider,
            IUserInfoProvider userInfoProvider)
            :base(securityProvider, userInfoProvider)
        {
            _securityRepository = securityRepository;
        }

        public IQueryable<Bonus> Filter(IQueryable<Bonus> data, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.AllowedBrands)
                .Single(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return data;

            var allowedBrandIds = user.AllowedBrands.Select(ab => ab.Id);

            return data.Where(b => allowedBrandIds.Contains(b.Template.Info.Brand.Id));
        }

        public IQueryable<Template> Filter(IQueryable<Template> data, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.AllowedBrands)
                .Single(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return data;

            var allowedBrandIds = user.AllowedBrands.Select(ab => ab.Id);

            return data.Where(t => allowedBrandIds.Contains(t.Info.Brand.Id));
        }

        public IQueryable<BonusRedemption> Filter(IQueryable<BonusRedemption> data, Guid userId)
        {
            var user = _securityRepository
                .Users
                .Include(x => x.Role)
                .Include(x => x.AllowedBrands)
                .Single(u => u.Id == userId);

            if (user.Role.Id == RoleIds.SuperAdminId)
                return data;

            var allowedBrandIds = user.AllowedBrands.Select(ab => ab.Id);

            return data.Where(br => allowedBrandIds.Contains(br.Bonus.Template.Info.Brand.Id));
        }
    }
}
