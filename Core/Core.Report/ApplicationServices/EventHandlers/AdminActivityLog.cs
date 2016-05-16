using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Brand.Events.ContentTranslation;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Bonus;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Events.Game;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Events;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Report.Events;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Events;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using BrandLanguagesAssigned = AFT.RegoV2.Core.Common.Events.Brand.BrandLanguagesAssigned;

namespace AFT.RegoV2.ApplicationServices.Report.EventHandlers
{
    public class AdminActivityLogEventHandlers : MarshalByRefObject
    {
        private readonly IUnityContainer _container;

        public AdminActivityLogEventHandlers(IUnityContainer container)
        {
            _container = container;
        }

        #region Player

        public void Handle(PlayerUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerStatusChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, "Player status changed", @event, @event.EventCreatedBy, null);
        }

        public void Handle(PlayerRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, "Player registered", @event, @event.EventCreatedBy, null);
        }

        public void Handle(NewPasswordSent @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        public void Handle(PlayerAccountRestrictionsChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Player, @event);
        }

        #endregion

        #region Brand category

        public void Handle(BrandRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event, @event.CreatedBy);
        }

        public void Handle(WithdrawalCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.WithdrawalCreated, @event, @event.CreatedBy);
        }

        public void Handle(BrandUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event, @event.UpdatedBy);
        }

        public void Handle(BrandActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event, @event.ActivatedBy);
        }

        public void Handle(BrandDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event, @event.DeactivatedBy);
        }

        public void Handle(BrandCountriesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandCurrenciesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandLanguagesAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }

        public void Handle(BrandProductsAssigned @event)
        {
            AddActivityLog(AdminActivityLogCategory.Brand, @event);
        }
        #endregion

        #region Licensee category

        public void Handle(LicenseeCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event, @event.CreatedBy);
        }

        public void Handle(LicenseeUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event, @event.UpdatedBy);
        }

        public void Handle(LicenseeActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event, @event.ActivatedBy);
        }

        public void Handle(LicenseeDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Licensee, @event, @event.DeactivatedBy);
        }
        #endregion

        #region Currency

        public void Handle(CurrencyCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event, @event.CreatedBy);
        }

        public void Handle(CurrencyUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event, @event.UpdatedBy);
        }

        public void Handle(CurrencyStatusChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Currency, @event, @event.StatusChangedBy);
        }

        #endregion

        #region Language
        public void Handle(LanguageCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event, @event.CreatedBy);
        }

        public void Handle(LanguageUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event, @event.UpdatedBy);
        }

        public void Handle(LanguageStatusChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.Language, @event, @event.StatusChangedBy);
        }
        #endregion

        #region Country
        public void Handle(CountryCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }

        public void Handle(CountryUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }

        public void Handle(CountryRemoved @event)
        {
            AddActivityLog(AdminActivityLogCategory.Country, @event);
        }
        #endregion

        #region VIP Level
        public void Handle(VipLevelRegistered @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event, @event.CreatedBy);
        }

        public void Handle(VipLevelUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event, @event.UpdatedBy);
        }

        public void Handle(VipLevelActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }

        public void Handle(VipLevelDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.VipLevel, @event);
        }
        #endregion

        #region Banks

        public void Handle(BankAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bank, @event);
        }

        public void Handle(BankEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bank, @event);
        }

        #endregion

        #region Player Bank Account
        public void Handle(PlayerBankAccountAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account added", @event, @event.CreatedBy, null);
        }

        public void Handle(PlayerBankAccountEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account edited", @event, @event.UpdatedBy, null);
        }

        public void Handle(PlayerBankAccountVerified @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account verified", @event, @event.VerifiedBy, @event.Remarks);
        }

        public void Handle(PlayerBankAccountRejected @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerBankAccount, "Player bank account rejected", @event, @event.RejectedBy, @event.Remarks);
        }
        #endregion

        #region Bank Account

        public void Handle(BankAccountAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        public void Handle(BankAccountDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BankAccount, @event);
        }

        #endregion

        #region Backend IP Regulation
        public void Handle(AdminIpRegulationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }

        public void Handle(AdminIpRegulationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }

        public void Handle(AdminIpRegulationDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.BackendIPRegulation, @event);
        }
        #endregion

        #region Player IP Regulation
        public void Handle(BrandIpRegulationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }

        public void Handle(BrandIpRegulationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }

        public void Handle(BrandIpRegulationDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.PlayerIPRegulation, @event);
        }
        #endregion

        #region Report
        public void Handle(ReportExported @event)
        {
            AddActivityLog(AdminActivityLogCategory.Report, @event);
        }
        #endregion

        #region Fraud Risk Level
        public void Handle(RiskLevelCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelStatusUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelTagPlayer @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        public void Handle(RiskLevelUntagPlayer @event)
        {
            AddActivityLog(AdminActivityLogCategory.FraudRiskLevel, @event);
        }
        #endregion

        #region Common
        protected void AddActivityLog(AdminActivityLogCategory category, IDomainEvent @event, string performedBy = null)
        {
            var defaultPropertyNames = typeof(IDomainEvent).GetProperties().Select(pi => pi.Name);
            var customProperties = @event.GetType().GetProperties().Where(pi => !defaultPropertyNames.Contains(pi.Name));
            var activityName = @event.GetType().Name.SeparateWords();
            var remark = GetRemark(@event, customProperties);

            AddActivityLog(category, activityName, @event, performedBy ?? GetUserName(@event.EventCreatedBy), remark);
        }

        private static string GetRemark(IDomainEvent @event, IEnumerable<PropertyInfo> customProperties)
        {
            return string.Join("\n", customProperties
                .Where(pi => pi.GetValue(@event) != null)
                .Select(pi =>
                {
                    var body = GetStringValue(@event, pi);

                    return pi.Name + ": " + body;
                }));
        }

        private static string GetStringValue(IDomainEvent @event, PropertyInfo pi)
        {
            var value = pi.GetValue(@event);

            if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
            {
                if (pi.PropertyType == typeof(DateTime))
                    return ((DateTime)value).GetNormalizedDateTimeWithOffset();

                if (pi.PropertyType == typeof(DateTimeOffset))
                    return ((DateTimeOffset)value).LocalDateTime.GetNormalizedDateTimeWithOffset();

                return value.ToString();
            }

            return JsonConvert.SerializeObject(value);
        }

        protected void AddActivityLog(AdminActivityLogCategory category, string activityName,
            IDomainEvent @event, string performedBy, string remarks)
        {
            var repository = _container.Resolve<IReportRepository>();
            repository.AdminActivityLog.Add(new AdminActivityLog
            {
                Id = Guid.NewGuid(),
                Category = category,
                ActivityDone = activityName,
                DatePerformed = @event.EventCreated,
                PerformedBy = performedBy,
                Remarks = remarks ?? string.Empty
            });
            repository.SaveChanges();
        }

        private string GetUserName(string performedBy)
        {
            var security = _container.Resolve<ISecurityRepository>();

            var user = string.IsNullOrWhiteSpace(performedBy)
                 ? null
                 : security.Users.SingleOrDefault(
                     u =>
                         // String.IsNullOrWhiteSpace in LINQ Expression, please refer to http://stackoverflow.com/questions/9606979/string-isnullorwhitespace-in-linq-expression
                         // 1)
                         //!string.IsNullOrWhiteSpace(u.Username) &&
                         u.Username != null && u.Username.Trim() != string.Empty &&
                         u.Username.Equals(performedBy, StringComparison.CurrentCultureIgnoreCase));
            // 2)
            //performedBy.Equals(u.Username, StringComparison.CurrentCultureIgnoreCase));

            var username = user != null ? user.Username : performedBy;

            return string.IsNullOrWhiteSpace(username) ? "System" : username;
        }
        #endregion

        #region Content Translations
        public void Handle(ContentTranslationCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.ContentTranslation, @event);
        }

        public void Handle(ContentTranslationUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.ContentTranslation, @event);
        }
        #endregion

        #region Transfer Fund Settings

        public void Handle(TransferFundSettingsActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.TransferFundSettings, @event);
        }

        public void Handle(TransferFundSettingsDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.TransferFundSettings, @event);
        }

        #endregion

        #region Payment Level

        public void Handle(PaymentLevelAdded @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelEdited @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        public void Handle(PaymentLevelDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentLevel, @event);
        }

        #endregion

        #region Payment Settings

        public void Handle(PaymentSettingCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        public void Handle(PaymentSettingDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.PaymentSetings, @event);
        }

        #endregion

        #region Bonus

        public void Handle(BonusCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusTemplateCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusTemplateUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        public void Handle(BonusIssuedByCs @event)
        {
            AddActivityLog(AdminActivityLogCategory.Bonus, @event);
        }

        #endregion

        #region Deposit

        public void Handle(DepositVerified @event)
        {
            AddActivityLog(AdminActivityLogCategory.OfflineDeposit, @event);
        }

        public void Handle(DepositUnverified @event)
        {
            AddActivityLog(AdminActivityLogCategory.OfflineDeposit, @event);
        }

        #endregion

        #region Role

        public void Handle(RoleUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Role, @event);
        }

        public void Handle(RoleCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Role, @event);
        }

        #endregion

        #region User

        public void Handle(UserCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(UserUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(UserActivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(UserDeactivated @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        public void Handle(UserPasswordChanged @event)
        {
            AddActivityLog(AdminActivityLogCategory.User, @event);
        }

        #endregion

        #region Game

        public void Handle(GameCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        public void Handle(GameUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        public void Handle(GameDeleted @event)
        {
            AddActivityLog(AdminActivityLogCategory.Game, @event);
        }

        #endregion

        #region Product

        public void Handle(ProductCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, @event);
        }

        public void Handle(ProductUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.Product, @event);
        }

        #endregion

        #region IdentificationDocumentSettings
        public void Handle(IdentificationDocumentSettingsCreated @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentificationDocumentSettings, @event);
        }

        public void Handle(IdentificationDocumentSettingsUpdated @event)
        {
            AddActivityLog(AdminActivityLogCategory.IdentificationDocumentSettings, @event);
        }
        #endregion

        public void Handle(DepositSubmitted @event)
        {
            AddActivityLog(AdminActivityLogCategory.OfflineDeposit, @event);
        }
    }
}
