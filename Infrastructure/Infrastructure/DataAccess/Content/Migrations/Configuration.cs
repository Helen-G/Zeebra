using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Content.Migrations
{
    public class Configuration : DbMigrationsConfiguration<ContentRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"DataAccess\Content\Migrations";
        }
    }
}