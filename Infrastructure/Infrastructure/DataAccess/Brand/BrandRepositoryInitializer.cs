using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Brand.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Brand
{
    public class BrandRepositoryInitializer : MigrateDatabaseToLatestVersion<BrandRepository, Configuration>
    {
    }
}