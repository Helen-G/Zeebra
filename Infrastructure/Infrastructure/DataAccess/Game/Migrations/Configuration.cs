using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Game.Migrations
{
    /// <summary>
    /// To re-scaffold initial migration please use the following command:
    /// add-migration initial -ConfigurationTypeName AFT.RegoV2.Infrastructure.DataAccess.Game.Migrations.Configuration -Force
    /// </summary>
    public sealed class Configuration : DbMigrationsConfiguration<GameRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            MigrationsDirectory = @"DataAccess\Game\Migrations";
        }
    }
}

