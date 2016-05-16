using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Report.Data.Brand;

namespace AFT.RegoV2.Infrastructure.DataAccess.Report.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ReportRepository>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = @"Common\Repository\Migrations";
        }
    }
}
