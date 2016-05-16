using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Infrastructure.Providers.Security;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using Moq;

namespace AFT.RegoV2.Tests.Common
{
    public class SingleProcessTestContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            //we are mocking only out-of-process components of the system, like database, filesystem, message serviceBus
            container.RegisterInstance<IFileStorage>(new Mock<IFileStorage>().Object);

            container.RegisterType<IServiceBus, FakeServiceBus>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBrandRepository, FakeBrandRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPlayerRepository, FakePlayerRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPaymentRepository, FakePaymentRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBonusRepository, FakeBonusRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFraudRepository, FakeFraudRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISecurityRepository, FakeSecurityRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IGameRepository, FakeGameRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEventRepository, FakeEventRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportRepository, FakeReportRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFraudRepository, FakeFraudRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentRepository, FakeContentsRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEmailNotifier, FakeEmailNotifier>(new ContainerControlledLifetimeManager());

            //TestServiceBusSubscriber publishes events to fake service bus
            var domainBus = container.Resolve<IEventBus>();
            domainBus.Subscribe<TestServiceBusSubscriber>(subscriberFactory: () => container.Resolve<TestServiceBusSubscriber>());
        }

        protected override void RegisterSessionProvider(IUnityContainer container)
        {
            container.RegisterType<ISessionProvider, TestSessionProvider>(new ContainerControlledLifetimeManager());
        }
    }
}