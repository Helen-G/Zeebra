using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Report.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Report
{
    public class ReportRepositoryInitializer : MigrateDatabaseToLatestVersion<ReportRepository, Configuration>
    {
    }
}