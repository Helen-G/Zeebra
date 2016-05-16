using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{

    public class FakeBrandRepository : IBrandRepository
    {
        private readonly FakeDbSet<Country> _countries = new FakeDbSet<Country>();
        private readonly FakeDbSet<Currency> _currencies = new FakeDbSet<Currency>();
        private readonly FakeDbSet<Culture> _cultures = new FakeDbSet<Culture>();
        private readonly FakeDbSet<Licensee> _licensees = new FakeDbSet<Licensee>();
        private readonly FakeFindableDbSet<Brand> _brands = new FakeFindableDbSet<Brand>((d,o) => o.SingleOrDefault(x => d.Contains(x.Id)));
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<RiskLevel> _riskLevels = new FakeDbSet<RiskLevel>();
        private readonly FakeDbSet<WalletTemplate> _walletTemplates = new FakeDbSet<WalletTemplate>();
        private readonly FakeDbSet<WalletTemplateProduct> _walletTemplateProducts = new FakeDbSet<WalletTemplateProduct>();
        private readonly FakeDbSet<ContentTranslation> _contentTranslation = new FakeDbSet<ContentTranslation>();
        private readonly FakeDbSet<VipLevelGameProviderBetLimit> _vipLevelLimits = new FakeDbSet<VipLevelGameProviderBetLimit>();

        public IDbSet<RiskLevel> RiskLevels { get { return _riskLevels; } }

        public IDbSet<WalletTemplateProduct> WalletTemplateProducts { get { return _walletTemplateProducts; } }

        public IDbSet<WalletTemplate> WalletTemplates { get { return _walletTemplates; } }

        public  IDbSet<Country> Countries { get { return _countries; } }

        public IDbSet<Currency> Currencies { get { return _currencies; } }

        public IDbSet<Culture> Cultures { get { return _cultures; } }

        public IDbSet<Licensee> Licensees { get { return _licensees; } }

        public IDbSet<Brand> Brands { get { return _brands; } }

        public IDbSet<VipLevel> VipLevels { get { return _vipLevels; } }

        public IDbSet<ContentTranslation> ContentTranslations { get { return _contentTranslation; } }

        public IDbSet<VipLevelGameProviderBetLimit> VipLevelLimits { get { return _vipLevelLimits; } }

        public int SaveChanges()
        {
            foreach (var vipLevel in _vipLevels)
            {
                if (vipLevel.Brand == null)
                    vipLevel.Brand = _brands.Single(x => x.Id == vipLevel.BrandId);
            }

            return 0;
        }
    }
}
