using System.Data.Entity;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Brand.Data;

namespace AFT.RegoV2.Core.Brand
{
    public interface IBrandRepository
    {
        IDbSet<Country>                 Countries { get; }
        IDbSet<Currency>                Currencies { get; }
        IDbSet<Culture>                 Cultures { get; }
        IDbSet<Data.Brand>              Brands { get; }
        IDbSet<Licensee>                Licensees { get; }
        IDbSet<VipLevel>                VipLevels { get; }
        IDbSet<WalletTemplate>          WalletTemplates { get; }
        IDbSet<RiskLevel>               RiskLevels { get; }
        IDbSet<WalletTemplateProduct>   WalletTemplateProducts { get; }
        IDbSet<ContentTranslation>      ContentTranslations { get; }
        IDbSet<VipLevelGameProviderBetLimit>        VipLevelLimits { get;  } 

        int SaveChanges();
    }
}