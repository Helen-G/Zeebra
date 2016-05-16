using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq.Expressions;
using log4net;

namespace AFT.RegoV2.Infrastructure.Synchronization
{
    public class FileTablesInitializer
    {
        private readonly ILog _logger;
        private readonly string _masterConnectionString;
        private readonly string databaseName;

        public FileTablesInitializer(ILog logger)
        {
            _logger = logger;
            var defaulConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaulConnectionString);
            databaseName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "Master";
            _masterConnectionString = connectionStringBuilder.ConnectionString;
        }

        public void Execute()
        {
            SqlConnection.ClearAllPools();

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();

                var context = new DbContext(connection, false);
                context.Database.Initialize(false);

                try
                {

                    context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                        string.Format(@"
use [{0}]

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Documents'))
BEGIN
    USE Master

    DECLARE @uniqueDirName nvarchar(max) = CONVERT(nvarchar(max), NEWID())
    exec('ALTER DATABASE [{0}]
    SET FILESTREAM (NON_TRANSACTED_ACCESS = FULL, DIRECTORY_NAME = '''+@uniqueDirName+''')')

    ALTER DATABASE [{0}]
    ADD FILEGROUP [Documents]
    CONTAINS FILESTREAM

    DECLARE @dataPath nvarchar(max) = CONVERT( nvarchar(max), SERVERPROPERTY('InstanceDefaultDataPath')) + N'{0}-Documents'
    exec('ALTER DATABASE [{0}] 
    ADD FILE(NAME = N''Files'', FILENAME = ''' + @dataPath + ''')
    TO FILEGROUP [Documents]')

    exec('USE [{0}]
    CREATE TABLE Documents AS FileTable')
END", databaseName));
                }
                catch (SqlException ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
        }
    }
}
