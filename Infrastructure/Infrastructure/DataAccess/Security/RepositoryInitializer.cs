using System.Data.Entity;

namespace AFT.RegoV2.Core.Services.Security
{
    public class RepositoryInitializer : MigrateDatabaseToLatestVersion<SecurityRepository, Configuration>
    {
    }
}
