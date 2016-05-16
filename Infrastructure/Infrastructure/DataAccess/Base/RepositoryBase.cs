using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace AFT.RegoV2.Infrastructure.DataAccess.Base
{
    public class RepositoryBase
    {
        public static bool IsDatabaseSeeded()
        {
            using (var db = new DbContext(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                // Most modern RDBMS (except Oracle) supports information_schema table
                DbRawSqlQuery<int> result = db.Database.SqlQuery<int>("SELECT COUNT(*) from information_schema.tables WHERE table_type = 'base table'");

                return result.First() > 2;
            }
        }
    }
}
