using System.Data.Entity.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Bonus.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<BonusRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"DataAccess\Bonus\Migrations";
        }
    }

    // To rebuild the InitialCreate file use this command in the PM Console:
    //
    // Add-Migration InitialCreate -ConfigurationTypeName AFT.RegoV2.Infrastructure.DataAccess.Bonus.Migrations.Configuration -Force
    //
    // (set Infrastructure as Default Project)
}