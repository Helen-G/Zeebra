using AFT.RegoV2.AdminWebsite;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Containers
{
    public class AdminWebsiteUnitTestContainerFactory : IContainerFactory
    {
        private readonly AdminWebsiteContainerFactory   _adminWebsiteContainerFactory;
        private readonly SingleProcessTestContainerFactory       _singleProcessTestContainerFactory;

        public AdminWebsiteUnitTestContainerFactory()
        {
            //we're not doing injection here, as it is factory already and it's very unlikely that we may need injection here
            _adminWebsiteContainerFactory = new AdminWebsiteContainerFactory();
            _singleProcessTestContainerFactory = new SingleProcessTestContainerFactory();
        }

        public IUnityContainer CreateWithRegisteredTypes()
        {
            var container = new UnityContainer();
            _adminWebsiteContainerFactory.RegisterTypes(container);
            _singleProcessTestContainerFactory.RegisterTypes(container);
            this.RegisterTypes(container);
            return container;
        }

        public void RegisterTypes(IUnityContainer container)
        {
            //those types need to be registered as singletones in our unit-tests
            container.RegisterType<WithdrawalService, WithdrawalService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWalletQueries, WalletQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWalletCommands, WalletCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrandQueries, BrandQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameCommands, GameCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameQueries, GameQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessageTemplatesQueries, MessageTemplatesQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMessageTemplatesCommands, MessageTemplatesCommands>(new ContainerControlledLifetimeManager());
        }
    }
}