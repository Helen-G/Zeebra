using AFT.RegoV2.ApplicationServices.Report.EventHandlers;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Events.ContentTranslation;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Bonus;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Events.Game;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Events;
using AFT.RegoV2.Domain.Security.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class AdminActivityLogWorker : WorkerBase
    {
        private readonly AdminActivityLogEventHandlers _eventHandlers;

        public AdminActivityLogWorker(
            IUnityContainer container,
            AdminActivityLogEventHandlers eventHandlers
            )
            : base(container)
        {
            _eventHandlers = eventHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            // Player
            RegisterEventHandler<PlayerUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerStatusChanged>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerRegistered>(_eventHandlers.Handle);
            RegisterEventHandler<NewPasswordSent>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerAccountRestrictionsChanged>(_eventHandlers.Handle);

            // Brand category
            RegisterEventHandler<BrandRegistered>(_eventHandlers.Handle);
            RegisterEventHandler<BrandUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<BrandActivated>(_eventHandlers.Handle);
            RegisterEventHandler<BrandDeactivated>(_eventHandlers.Handle);
            RegisterEventHandler<BrandCountriesAssigned>(_eventHandlers.Handle);
            RegisterEventHandler<BrandCurrenciesAssigned>(_eventHandlers.Handle);
            RegisterEventHandler<BrandLanguagesAssigned>(_eventHandlers.Handle);
            RegisterEventHandler<BrandProductsAssigned>(_eventHandlers.Handle);

            // Licensee category
            RegisterEventHandler<LicenseeCreated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeActivated>(_eventHandlers.Handle);
            RegisterEventHandler<LicenseeDeactivated>(_eventHandlers.Handle);

            // Currency category
            RegisterEventHandler<CurrencyCreated>(_eventHandlers.Handle);
            RegisterEventHandler<CurrencyUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<CurrencyStatusChanged>(_eventHandlers.Handle);

            // Language category
            RegisterEventHandler<LanguageCreated>(_eventHandlers.Handle);
            RegisterEventHandler<LanguageUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<LanguageStatusChanged>(_eventHandlers.Handle);

            // Country category
            RegisterEventHandler<CountryCreated>(_eventHandlers.Handle);
            RegisterEventHandler<CountryUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<CountryRemoved>(_eventHandlers.Handle);

            // VIP Level category
            RegisterEventHandler<VipLevelRegistered>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelActivated>(_eventHandlers.Handle);
            RegisterEventHandler<VipLevelDeactivated>(_eventHandlers.Handle);

            // Backend IP Regulation category
            RegisterEventHandler<AdminIpRegulationCreated>(_eventHandlers.Handle);
            RegisterEventHandler<AdminIpRegulationUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<AdminIpRegulationDeleted>(_eventHandlers.Handle);

            // Player IP Regulation category
            RegisterEventHandler<BrandIpRegulationCreated>(_eventHandlers.Handle);
            RegisterEventHandler<BrandIpRegulationUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<BrandIpRegulationDeleted>(_eventHandlers.Handle);

            // Report category
            RegisterEventHandler<ReportExported>(_eventHandlers.Handle);

            //Withdrawal
            RegisterEventHandler<WithdrawalCreated>(_eventHandlers.Handle);

            // Player Bank Account
            RegisterEventHandler<PlayerBankAccountAdded>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerBankAccountEdited>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerBankAccountVerified>(_eventHandlers.Handle);
            RegisterEventHandler<PlayerBankAccountRejected>(_eventHandlers.Handle);

            // Bank Account
            RegisterEventHandler<BankAccountAdded>(_eventHandlers.Handle);
            RegisterEventHandler<BankAccountEdited>(_eventHandlers.Handle);
            RegisterEventHandler<BankAccountActivated>(_eventHandlers.Handle);
            RegisterEventHandler<BankAccountDeactivated>(_eventHandlers.Handle);

            // Bank
            RegisterEventHandler<BankAdded>(_eventHandlers.Handle);
            RegisterEventHandler<BankEdited>(_eventHandlers.Handle);

            // Content Translation
            RegisterEventHandler<ContentTranslationCreated>(_eventHandlers.Handle);
            RegisterEventHandler<ContentTranslationUpdated>(_eventHandlers.Handle);

            // fraud risk level
            RegisterEventHandler<RiskLevelCreated>(_eventHandlers.Handle);
            RegisterEventHandler<RiskLevelUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<RiskLevelStatusUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<RiskLevelTagPlayer>(_eventHandlers.Handle);
            RegisterEventHandler<RiskLevelUntagPlayer>(_eventHandlers.Handle);

            // Transfer Fund Settings
            RegisterEventHandler<TransferFundSettingsActivated>(_eventHandlers.Handle);
            RegisterEventHandler<TransferFundSettingsDeactivated>(_eventHandlers.Handle);

            // Payment Level
            RegisterEventHandler<PaymentLevelAdded>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentLevelEdited>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentLevelActivated>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentLevelDeactivated>(_eventHandlers.Handle);

            // Offline Deposit
            RegisterEventHandler<DepositVerified>(_eventHandlers.Handle);
            RegisterEventHandler<DepositUnverified>(_eventHandlers.Handle);

            // Payment Settings
            RegisterEventHandler<PaymentSettingActivated>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentSettingDeactivated>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentSettingCreated>(_eventHandlers.Handle);
            RegisterEventHandler<PaymentSettingUpdated>(_eventHandlers.Handle);

            // Bonus
            RegisterEventHandler<BonusCreated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusActivated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusDeactivated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusTemplateCreated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusTemplateUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<BonusIssuedByCs>(_eventHandlers.Handle);

            // Role
            RegisterEventHandler<RoleCreated>(_eventHandlers.Handle);
            RegisterEventHandler<RoleUpdated>(_eventHandlers.Handle);

            // User
            RegisterEventHandler<UserCreated>(_eventHandlers.Handle);
            RegisterEventHandler<UserUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<UserActivated>(_eventHandlers.Handle);
            RegisterEventHandler<UserDeactivated>(_eventHandlers.Handle);
            RegisterEventHandler<UserPasswordChanged>(_eventHandlers.Handle);

            // Deposit
            RegisterEventHandler<DepositSubmitted>(_eventHandlers.Handle);

            // Game
            RegisterEventHandler<GameCreated>(_eventHandlers.Handle);
            RegisterEventHandler<GameUpdated>(_eventHandlers.Handle);
            RegisterEventHandler<GameDeleted>(_eventHandlers.Handle);

            // Product
            RegisterEventHandler<ProductCreated>(_eventHandlers.Handle);
            RegisterEventHandler<ProductUpdated>(_eventHandlers.Handle);

            // Identification Documents
            RegisterEventHandler<IdentificationDocumentSettingsCreated>(_eventHandlers.Handle);
            RegisterEventHandler<IdentificationDocumentSettingsUpdated>(_eventHandlers.Handle);
        }
    }
}
