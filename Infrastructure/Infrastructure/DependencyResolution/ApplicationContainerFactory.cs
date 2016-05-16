using System;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Player.ApplicationServices;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Event;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Services;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.ApplicationServices;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Aspects;
using AFT.RegoV2.Infrastructure.Crypto;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Infrastructure.DataAccess.Content;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using AFT.RegoV2.Infrastructure.DataAccess.Fraud;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Infrastructure.DataAccess.Player;
using AFT.RegoV2.Infrastructure.DataAccess.Report;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Providers;
using AFT.RegoV2.Infrastructure.Mail;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.Infrastructure.Providers.Security;
using AFT.RegoV2.Infrastructure.Sms;
using AFT.RegoV2.Infrastructure.Synchronization;
using Core.Player.ApplicationServices;
using log4net;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using IJsonSerializationProvider = AFT.RegoV2.Infrastructure.Providers.IJsonSerializationProvider;
using IServiceBus = MassTransit.IServiceBus;
using ITokenProvider = AFT.RegoV2.Infrastructure.Providers.ITokenProvider;
using ITransactionScopeProvider = AFT.RegoV2.Infrastructure.Providers.ITransactionScopeProvider;
using IWebConfigProvider = AFT.RegoV2.Infrastructure.Providers.IWebConfigProvider;
using JsonSerializationProvider = AFT.RegoV2.Infrastructure.Providers.JsonSerializationProvider;
using PaymentSubscriber = AFT.RegoV2.ApplicationServices.Payment.PaymentSubscriber;
using TokenProvider = AFT.RegoV2.Infrastructure.Providers.TokenProvider;
using TransactionScopeProvider = AFT.RegoV2.Infrastructure.Providers.TransactionScopeProvider;
using WebConfigProvider = AFT.RegoV2.Infrastructure.Providers.WebConfigProvider;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public interface IContainerFactory
    {
        IUnityContainer CreateWithRegisteredTypes();
        void RegisterTypes(IUnityContainer container);
    }

    /// <summary>
    /// Provides application-wide initialization logic for IoC container
    /// </summary>
    public class ApplicationContainerFactory
    {
        public IUnityContainer CreateWithRegisteredTypes()
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        }

        public virtual void RegisterTypes(IUnityContainer container)
        {
            RegisterInfrastructureTypes(container);

            RegisterReportTypes(container);

            RegisterBrandTypes(container);

            RegisterContentTranslationTypes(container);

            RegisterSecurityTypes(container);

            RegisterPlayerTypes(container);

            RegisterPaymentTypes(container);

            RegisterFraudTypes(container);

            RegisterBonusTypes(container);

            RegisterMessagingTypes(container);

            RegisterWalletTypes(container);

            RegisterGameTypes(container);

            //TODO: consider moving out this line to some other place as it has side effects
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        public virtual InterceptionBehavior GetSecurityInterceptionBehavior()
        {
            return new InterceptionBehavior<DummyInterceptionBehavior>();
        }

        protected virtual InterceptionBehavior GetBrandCheckAspect()
        {
            return new InterceptionBehavior<DummyInterceptionBehavior>();
        }

        protected virtual void RegisterSessionProvider(IUnityContainer container)
        {
            container.RegisterType<ISessionProvider, TestSessionProvider>(new ContainerControlledLifetimeManager());
        }

        private void RegisterContentTranslationTypes(IUnityContainer container)
        {
            container.RegisterType<ContentTranslationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<ContentTranslationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
        }

        private void RegisterReportTypes(IUnityContainer container)
        {
            container.RegisterType<IReportRepository, ReportRepository>();
            container.RegisterType<ReportQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            // Player Reports
            container.RegisterType<PlayerReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<PlayerBetHistoryReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            // Payment Reports
            container.RegisterType<DepositReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            // Brand Reports
            container.RegisterType<BrandReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<LicenseeReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<VipLevelReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<LanguageReportEventHandlers>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
        }

        private void RegisterBrandTypes(IUnityContainer container)
        {
            container.RegisterType<IBrandRepository, BrandRepository>(new PerHttpRequestLifetime());
            container.RegisterType<ILicenseeCommands, LicenseeCommands>();
            container.RegisterType<IBrandCommands, BrandCommands>();
            container.RegisterType<ICultureCommands, CultureCommands>();

        }

        private void RegisterSecurityTypes(IUnityContainer container)
        {
            container.RegisterType<ISecurityRepository, SecurityRepository>(new PerHttpRequestLifetime());
            container.RegisterType<ISecurityProvider, SecurityProvider>();
            container.RegisterType<IUserInfoProvider, UserInfoProvider>();

            container.RegisterType<OfflineDepositCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                ); container.RegisterType<IPermissionService, PermissionService>();

            container.AddNewExtension<Interception>();

            container.RegisterType<UserService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<RoleService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BackendIpRegulationService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BrandIpRegulationService>(
                new PerHttpRequestLifetime(),
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<BrandQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                new InterceptionBehavior<BrandFilterAspect>()
                );

            container.RegisterType<BrandCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PaymentQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BankAccountCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<BankAccountQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<BankCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<PlayerBankAccountCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PlayerBankAccountQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<OfflineDepositCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PlayerQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PlayerCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<LoggingService>(new PerHttpRequestLifetime());
            container.RegisterType<PlayerActivityLogEventHandlersBase>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<LicenseeCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<LicenseeQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<GameCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<ReportQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );

            container.RegisterType<PaymentLevelQueries>(
               new Interceptor<TransparentProxyInterceptor>(),
               GetSecurityInterceptionBehavior()
               );

            container.RegisterType<PaymentLevelCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PaymentSettingsQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<PaymentSettingsCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );

            container.RegisterType<TransferSettingsCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
        }

        private void RegisterPlayerTypes(IUnityContainer container)
        {
            container.RegisterType<IPlayerQueries, PlayerQueries>();
            container.RegisterType<IPlayerRepository, PlayerRepository>();
            container.RegisterType<IPlayerService, PlayerService>();
            container.RegisterType<IPlayerIdentityValidator, PlayerIdentityValidator>();
        }

        private void RegisterPaymentTypes(IUnityContainer container)
        {
            container.RegisterType<IPaymentRepository, PaymentRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IPaymentQueries, PaymentQueries>(
                GetBrandCheckAspect()
                );
            container.RegisterType<IPaymentSettingsValidationService, PaymentSettingsValidationService>();
            container.RegisterType<ITransferFundCommands, TransferFundCommands>();
            container.RegisterType<WithdrawalService>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IBasePaymentQueries, PaymentQueries>();
            container.RegisterType<ICurrencyCommands, CurrencyCommands>();
        }

        private void RegisterBonusTypes(IUnityContainer container)
        {
            container.RegisterType<IBonusRepository, BonusRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IBonusQueries, BonusQueries>();
            container.RegisterType<BonusQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                new InterceptionBehavior<BonusFilterAspect>()
                );
            container.RegisterType<BonusManagementCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
            );
        }

        private void RegisterFraudTypes(IUnityContainer container)
        {
            container.RegisterType<IFraudRepository, FraudRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IWagerConfigurationQueries, WagerConfigurationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior());
            container.RegisterType<IWagerConfigurationCommands, WagerConfigurationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect());
            container.RegisterType<ITransferFundValidationService, TransferFundValidationService>();
            container.RegisterType<IOfflineWithdrawalValidationService, OfflineWithdrawalValidationService>();
            container.RegisterType<IFundsValidationService, FundsValidationService>();
            container.RegisterType<IAWCValidationService, AWCValidationService>();
            container.RegisterType<IAVCValidationService, AVCValidationService>();
            container.RegisterType<IAVCConfigurationCommands, AVCConfigurationCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IAVCConfigurationQueries, AVCConfigurationQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IRebateWageringValidationService, RebateWageringValidationService>();
            container.RegisterType<IManualAdjustmentWageringValidationService, ManualAdjustmentWageringValidationService>();
            container.RegisterType<IBonusWageringWithdrawalValidationService, BonusWageringWithdrawalValidationService>();
            container.RegisterType<IRiskLevelQueries, RiskLevelQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IRiskLevelCommands, RiskLevelCommands>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior(),
                GetBrandCheckAspect()
                );
            container.RegisterType<IWinningRuleQueries, WinningRuleQueries>();
            container.RegisterType<IOfflineDepositValidator, OfflineDepositValidator>();
        }

        private void RegisterMessagingTypes(IUnityContainer container)
        {
            container.RegisterType<IContentRepository, ContentRepository>(new PerHttpRequestLifetime());
            container.RegisterType<IMessageTemplateService, MessageTemplateService>(new PerHttpRequestLifetime());
            container.RegisterType<IMessageTemplatesCommands, MessageTemplatesCommands>();
            container.RegisterType<IMessageTemplatesQueries, MessageTemplatesQueries>();
        }

        private void RegisterWalletTypes(IUnityContainer container)
        {
            container.RegisterType<IWalletCommands, WalletCommands>();
            container.RegisterType<IWalletQueries, WalletQueries>();
        }

        private void RegisterGameTypes(IUnityContainer container)
        {
            container.RegisterType<IJsonSerializationProvider, JsonSerializationProvider>();
            container.RegisterType<ITransactionScopeProvider, TransactionScopeProvider>();
            container.RegisterType<IGameRepository, GameRepository>();
            container.RegisterType<IWebConfigProvider, WebConfigProvider>();
            container.RegisterType<ICryptoProvider, CryptoProvider>();
            container.RegisterType<ITokenProvider, TokenProvider>();

            container.RegisterType<IGameQueries, GameQueries>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior()
                );
            container.RegisterType<IGameWalletOperations, GameWalletOperations>();
            container.RegisterType<IGameManagement, GameManagement>(
                new Interceptor<TransparentProxyInterceptor>(),
                GetSecurityInterceptionBehavior());
            container.RegisterType<IGameCommands, GameCommands>();
        }

        private void RegisterInfrastructureTypes(IUnityContainer container)
        {
            var log = LogManager.GetLogger(string.Empty);
            container.RegisterInstance<ILog>(log);

            RegisterEventBus(container);
            RegisterServiceBus(container);

            container.RegisterType<IFileStorage, FileSystemStorage>();
            container.RegisterType<IEmailNotifier, EmailNotifier>();
            container.RegisterType<ISmsNotifier, SmsNotifier>();
            container.RegisterType<IEventRepository, EventRepository>();

            RegisterSessionProvider(container);

            container.RegisterType<ISynchronizationService, SynchronizationService>();
        }

        private void RegisterServiceBus(IUnityContainer container)
        {
            container.RegisterType<Func<string, MassTransit.IServiceBus>>(new InjectionFactory(c =>
                new Func<string, MassTransit.IServiceBus>(MassTransitBusFactory.GetBus)
            ));
            container.RegisterType<Core.Common.Interfaces.IServiceBus, RabbitMqServiceBus>();
            container.RegisterType<ServiceBusPublisherCache>(new ContainerControlledLifetimeManager());
        }

        private void RegisterEventBus(IUnityContainer container)
        {
            var bus = new EventBus();
            bus.Subscribe(() => container.Resolve<EventStoreSubscriber>());
            bus.Subscribe(() => container.Resolve<PaymentSubscriber>());
            bus.Subscribe(() => container.Resolve<WalletSubscriber>());
            bus.Subscribe(() => container.Resolve<GameSubscriber>());
            bus.Subscribe(() => container.Resolve<PlayerSubscriber>());
            bus.Subscribe(() => container.Resolve<RiskLevelSubscriber>());
            bus.Subscribe(() => container.Resolve<SecuritySubscriber>());
            bus.Subscribe(() => container.Resolve<BrandSubscriber>());
            bus.Subscribe(() => container.Resolve<ContentSubscriber>());
            container.RegisterInstance<IEventBus>(bus, new ContainerControlledLifetimeManager());
        }
    }
}