using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<EventRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"Common\Repository\Migrations";
        }
    }
}
