using System;
using System.Web.Http.Dependencies;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.WinService.Workers;
using log4net;
using log4net.Config;
using Microsoft.Practices.Unity;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Unity.WebApi;
using WinService.Workers;

namespace AFT.RegoV2.WinService
{
    class WinServiceContainerFactory : ApplicationContainerFactory
    {
        public override void RegisterTypes(IUnityContainer container)
        {
            base.RegisterTypes(container);

            container.RegisterType<IJobFactory, UnityJobFactory>();
            container.RegisterInstance<ISchedulerFactory>(new StdSchedulerFactory());

            container.RegisterType<CompositeWorker>();
            container.RegisterType<IDependencyResolver, UnityDependencyResolver>();

            container.RegisterType<IWorker, SmsNotificationWorker>("SmsNotifications");
            container.RegisterType<IWorker, EmailNotificationWorker>("EmailNotifications");
            container.RegisterType<IWorker, BonusWorker>("BonusWorker");


            // Player Reports
            container.RegisterType<IWorker, PlayerReportWorker>("PlayerReport");
            container.RegisterType<IWorker, PlayerBetHistoryReportWorker>("PlayerBetHistoryReport");

            // Payment Reports
            container.RegisterType<IWorker, DepositReportWorker>("DepositReport");

            // Transaction Reports
            container.RegisterType<IWorker, PlayerTransactionReportWorker>("PlayerTransactionReport");

            // Brand Reports
            container.RegisterType<IWorker, BrandReportWorker>("BrandReport");
            container.RegisterType<IWorker, LicenseeReportWorker>("LicenseeReport");
            container.RegisterType<IWorker, LanguageReportWorker>("LanguageReport");
            container.RegisterType<IWorker, VipLevelReportWorker>("VipLevelReport");


            container.RegisterType<IWorker, AdminActivityLogWorker>("AdminActivityLog");
            container.RegisterType<IWorker, AuthenticationLogWorker>("AuthenticationLog");
            container.RegisterType<IWorker, PlayerActivityLogWorker>("PlayerActivityLog");

            container.RegisterType<IWorker, EventPublisherWorker>("EventPublisher");

            var logger = container.Resolve<ILog>();
            container.RegisterType<ILog>(new ContainerControlledLifetimeManager(), new InjectionFactory(c =>
            {
                XmlConfigurator.Configure();
                return logger;
            }));
        }
    }
}
