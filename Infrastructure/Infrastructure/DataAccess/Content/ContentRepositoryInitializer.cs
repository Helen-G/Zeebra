using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Content.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Content
{
    public class ContentRepositoryInitializer : MigrateDatabaseToLatestVersion<ContentRepository, Configuration>
    {
    }
}