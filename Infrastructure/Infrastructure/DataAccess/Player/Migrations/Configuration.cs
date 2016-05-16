using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player.Data;

namespace AFT.RegoV2.Infrastructure.DataAccess.Player.Repository.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<PlayerRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"Common\Repository\Migrations";
        }
    }
}