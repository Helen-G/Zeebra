using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common
{
    public class MemberWebsiteUnitTestContainerFactory
    {
        private readonly SingleProcessTestContainerFactory _singleProcessTestContainerFactory;

        public MemberWebsiteUnitTestContainerFactory()
        {
            _singleProcessTestContainerFactory = new SingleProcessTestContainerFactory();
        }

        public void RegisterTypes(IUnityContainer container)
        {
            _singleProcessTestContainerFactory.RegisterTypes(container);

            container.RegisterType<IFileStorage, FileSystemStorage>();
            container.RegisterType<WithdrawalService, WithdrawalService>(new ContainerControlledLifetimeManager());
            container.RegisterType<WalletQueries, WalletQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IWalletCommands, WalletCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<BrandQueries, BrandQueries>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameCommands, GameCommands>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameQueries, GameQueries>(new ContainerControlledLifetimeManager());
        }

        public IUnityContainer CreateWithRegisteredTypes()
        {
            return _singleProcessTestContainerFactory.CreateWithRegisteredTypes();
        }
    }
}