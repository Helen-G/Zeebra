using System.Web.Hosting;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.Infrastructure.Crypto;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.OAuth2;
using AFT.RegoV2.Infrastructure.Providers;
using log4net.Config;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.GameApi.Interface.Services
{
    public interface IGameApiFactory
    {
        IUnityContainer Container { get; }
    }
    public sealed class GameApiFactory : IGameApiFactory
    {
        public static readonly IGameApiFactory Default = new GameApiFactory();

        private readonly IUnityContainer _container = new ApplicationContainerFactory().CreateWithRegisteredTypes();

        private GameApiFactory()
        {
            SetupGameApiDependencies(_container);
        }
        IUnityContainer IGameApiFactory.Container { get { return _container; } }

        private void SetupGameApiDependencies(IUnityContainer container)
        {
            XmlConfigurator.Configure();

            container.RegisterInstance(container);
            container.RegisterType<IJsonSerializationProvider, JsonSerializationProvider>(ReuseWithinContainer);
            container.RegisterType<IWebConfigProvider, WebConfigProvider>(ReuseWithinContainer);
            container.RegisterType<ITokenProvider, TokenProvider>(ReuseWithinContainer);
            container.RegisterType<ICryptoProvider, CryptoProvider>(ReuseWithinContainer);
            container.RegisterType<IErrorManager, ErrorManager>(ReuseWithinContainer);
            container.RegisterType<ITokenValidationProvider, TokenValidationProvider>(ReuseWithinContainer);
            container.RegisterType<IGameProviderLog, GameProviderLog>(ReuseWithinContainer);
            container.RegisterType<ITransactionScopeProvider, TransactionScopeProvider>(ReuseWithinContainer);
            container.RegisterType<IGameRepository, GameRepository>(NewEachTime);
            container.RegisterType<IGameCommands, GameCommands>(NewEachTime);
            container.RegisterType<IGameQueries, GameQueries>(NewEachTime);
            container.RegisterType<IWalletCommands, WalletCommands>(NewEachTime);
            container.RegisterType<IGamesCommonOperationsProvider, GamesCommonOperationsProvider>(NewEachTime);

            //use of explicit factory method to prevent Func from doing property injection 
            //and stuffing your public properties with nulls (surprise!)
            container.RegisterType<IGameRepository>(ReuseWithinRequest,
                  new InjectionFactory(c => new GameRepository()));

            container.RegisterType<ICryptoKeyPair>("authServer", NewEachTime, new InjectionFactory(c =>
            {
                var config = c.Resolve<IWebConfigProvider>();
                return CryptoKeyPair.LoadCertificate(
                            HostingEnvironment.MapPath(config.GetAppSettingByKey("CertificateLocation")),
                            config.GetAppSettingByKey("CertificatePassword"));
            }));

            container.RegisterType<ICryptoKeyPair>("dataServer", NewEachTime, new InjectionFactory(c =>
            {
                var config = c.Resolve<IWebConfigProvider>();
                return CryptoKeyPair.LoadCertificate(
                            HostingEnvironment.MapPath(config.GetAppSettingByKey("CertificateLocation")), 
                            config.GetAppSettingByKey("CertificatePassword"));
            }));
        }
        private static LifetimeManager ReuseWithinContainer { get { return new ContainerControlledLifetimeManager(); } }
        private static LifetimeManager ReuseWithinRequest { get { return new PerHttpRequestLifetime(); } }
        private static LifetimeManager NewEachTime { get { return new PerResolveLifetimeManager(); } }
    }
}