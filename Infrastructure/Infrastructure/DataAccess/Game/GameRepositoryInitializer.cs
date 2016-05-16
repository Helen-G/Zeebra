using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Game.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game
{
    internal class GameRepositoryInitializer : MigrateDatabaseToLatestVersion<GameRepository, Configuration>
    {
    }
}
