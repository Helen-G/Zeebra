using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Repository.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Repository
{
    public class PlayerRepositoryInitializer : MigrateDatabaseToLatestVersion<PlayerRepository, Configuration>
    {
    }
}