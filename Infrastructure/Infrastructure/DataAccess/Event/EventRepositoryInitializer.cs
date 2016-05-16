using System.Data.Entity;
using AFT.RegoV2.Infrastructure.DataAccess.Event.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Event
{
    public class EventRepositoryInitializer : MigrateDatabaseToLatestVersion<EventRepository, Configuration>
    {
    }
}