using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web.Hosting;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Infrastructure.Synchronization
{
    /// <summary>
    ///  Lock service based on DB lock
    /// </summary>
    public class SynchronizationService : ISynchronizationService
    {
        private readonly string _masterConnectionString;
        private const int SynchronizationTimeOut = 180;

        public SynchronizationService()
        {
            var defaulConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaulConnectionString)
            {
                InitialCatalog = "Master",
            };
            _masterConnectionString = connectionStringBuilder.ConnectionString;
        }

        /// <summary>
        /// make sure you aware that total lock time can't be greater than SynchronizationTimeOut.
        /// default DbContext timeout 15 sec.
        /// </summary>
        public void Execute(string sectionName, Action action)
        {
            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();

                var context = new DbContext(connection, contextOwnsConnection: false);
                context.Database.CommandTimeout = SynchronizationTimeOut;
                
                try
                {
                    context.Database.ExecuteSqlCommand(
                        string.Format(
                            "EXEC sp_getapplock @Resource = '{0}', @LockMode = 'Exclusive', @LockOwner = 'Session';",
                            sectionName));

                    action();
                }
                finally
                {
                    context.Database.ExecuteSqlCommand(string.Format("EXEC sp_releaseapplock  @Resource = '{0}', @LockOwner = 'Session';", sectionName));
                }
            }
        }
    }
}
