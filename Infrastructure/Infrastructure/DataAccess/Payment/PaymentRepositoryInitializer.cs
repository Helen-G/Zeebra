using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository
{
    public class PaymentRepositoryInitializer : MigrateDatabaseToLatestVersion<PaymentRepository, Configuration>
    {
    }
}