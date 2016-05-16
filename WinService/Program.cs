using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess;
using AFT.RegoV2.Infrastructure.Synchronization;
using AFT.RegoV2.Shared;
using log4net;
using Microsoft.Practices.Unity;
using Topshelf;
using WinService.Workers;

namespace AFT.RegoV2.WinService
{
    internal class Program
    {
        private static IUnityContainer _container;
        private static ILog _logger;

        private static string WinServiceName
        {
            get { return ConfigurationManager.AppSettings["WinServiceName"]; }
        }

        private static void Main(string[] args)
        {
            _container = new WinServiceContainerFactory().CreateWithRegisteredTypes();
            _logger = _container.Resolve<ILog>();

            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<CompositeWorker>(
                    serviceConfigurator =>
                    {
                        serviceConfigurator.ConstructUsing(x => _container.Resolve<CompositeWorker>(
                            new ResolverOverride[]
                            {
                                new ParameterOverride("services", _container.ResolveAll<IWorker>().ToArray())
                            }));
                        serviceConfigurator.WhenStarted(x =>
                        {

                            InitializeAndSeedDatabases();
                            InitializeSQLServerSessionStore();
                            x.Start();
                        });
                        serviceConfigurator.WhenStopped(x => x.Stop());
                    });

                hostConfigurator.UseLog4Net(".\\WinService.exe.config");
                hostConfigurator.StartAutomatically();
                hostConfigurator.SetDescription("AFT REGOv2 background tasks execution service.");
                hostConfigurator.SetDisplayName(WinServiceName);
                hostConfigurator.SetServiceName(WinServiceName);
            });
        }

        private static void InitializeAndSeedDatabases()
        {
            _logger.Debug("Initializing databases ...");

            var synchronizationService = _container.Resolve<SynchronizationService>();
            synchronizationService.Execute("WinService", () =>
            {
                InitializeAllRepositories();
                InitializeFileTables();
                RunApplicationSeeder();
                SeedIndividualRepositories();
            });

            _logger.Debug("Initializing databases completed.");
        }

        private static void InitializeFileTables()
        {
            var initializer = new FileTablesInitializer(_logger);
            initializer.Execute();
        }

        // this method creates basic structure for all repositories by mostly creating empty tables, keys, indexes
        private static void InitializeAllRepositories()
        {
            _logger.Debug("Initializing repositories ...");

            AppDomain.CurrentDomain.GetRegoTypes()
                    .Where(t => t.IsClass && t.IsDescendentOf(typeof(DbContext)))
                    .ForEach(t =>
                    {
                        using (var repository = (DbContext) _container.Resolve(t))
                        {
                            repository.Database.Initialize(false);
                        }
                    });
            _logger.Debug("Initializing repositories completed.");
        }

        //this step is responsible for creating basic entities so that system can function properly
        private static void RunApplicationSeeder()
        {
            _logger.Debug("Running ApplicationSeeder ...");

            _container.Resolve<ApplicationSeeder>().Seed();

            _logger.Debug("Running ApplicationSeeder completed.");
        }

        // each individual repository may have some custom seeding logic which is not possible to describe through Fluent interface
        private static void SeedIndividualRepositories()
        {
            _logger.Debug("Seeding databases ...");

            AppDomain.CurrentDomain.GetRegoTypes()
                .Where(t => t.IsClass && t.IsDescendentOf(typeof(ISeedable)))
                .OrderByDescending(x => x.AssemblyQualifiedName)
                .ForEach(seedableRepository =>
                {
                    var seedable = (ISeedable)_container.Resolve(seedableRepository);
                    _logger.Debug("Seeding " + seedableRepository.Name);
                    seedable.Seed();
                });
            _logger.Debug("Seeding databases completed.");
        }

        private static void InitializeSQLServerSessionStore()
        {
            _logger.Debug("Initializing SQL Server Session Store Mode");

            var defaulConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            var connectionStringBuilder = new SqlConnectionStringBuilder(defaulConnectionString);
            var server = connectionStringBuilder.DataSource;
            var database = connectionStringBuilder.InitialCatalog;
            var user = connectionStringBuilder.UserID;
            var password = connectionStringBuilder.Password;
            var frameworkPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            var regsqlPath = frameworkPath + "aspnet_regsql.exe";

            var arguments = string.Format("-S {0} -U {1} -P {2} -ssadd -sstype c -d {3}", server, user, password, database);

            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = regsqlPath,
                    Arguments = arguments
                }
            };

            p.Start();

            var output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();

            Console.WriteLine(output);

            if (p.ExitCode != 0)
            {
                _logger.Debug("SQL Server Store initialization failed");
            }
        }
    }
}