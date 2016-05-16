using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using FluentValidation;

namespace AFT.RegoV2.Domain.Payment.ApplicationServices
{
    public class PaymentSettingsCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        public PaymentSettingsCommands(IPaymentRepository repository, ISecurityProvider securityProvider, IEventBus eventBus)
        {
            _repository = repository;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
        }

        public Guid AddSettings(SavePaymentSettingsCommand model)
        {
            var validationResult = new SavePaymentSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            if (_repository.PaymentSettings.Any(x => x.Id == model.Id))
            {
                throw new RegoException(PaymentSettingsErrors.AlreadyExistsError.ToString());
            }

            if (_repository.PaymentSettings.Any(
                x => x.BrandId == model.Brand
                    && x.VipLevel == model.VipLevel
                    && x.PaymentType == model.PaymentType
                    && x.PaymentGateway.BankAccount.Id == model.PaymentMethod))
            {
                throw new RegoException(PaymentSettingsErrors.AlreadyExistsError.ToString());
            }
            
            var bankAccount = _repository.BankAccounts.SingleOrDefault(x => x.Id == model.PaymentMethod);
            if (bankAccount == null)
            {
                throw new RegoException(PaymentSettingsErrors.BankAccountNotFound.ToString());
            }

            var paymentSettings = new PaymentSettings();
            paymentSettings.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            paymentSettings.BrandId = model.Brand;
            paymentSettings.CurrencyCode = model.Currency;
            paymentSettings.VipLevel = model.VipLevel;
            paymentSettings.PaymentType = model.PaymentType;
            paymentSettings.PaymentGateway = new PaymentGateway { Id = Guid.NewGuid(), BankAccount = bankAccount };
            paymentSettings.CreatedDate = DateTime.Now;
            paymentSettings.CreatedBy = _securityProvider.User.UserName;
            paymentSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            paymentSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            paymentSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            paymentSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            paymentSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            paymentSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            _repository.PaymentSettings.Add(paymentSettings);
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingCreated(paymentSettings));

            return paymentSettings.Id;
        }

        public void UpdateSettings(SavePaymentSettingsCommand model)
        {
            var validationResult = new SavePaymentSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var paymentSettings = _repository.PaymentSettings.SingleOrDefault(x => x.Id == model.Id);
            if (paymentSettings == null)
                throw new RegoException(string.Format("Payment settings with id '{0}' were not found", model.Id));

            paymentSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            paymentSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            paymentSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            paymentSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            paymentSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            paymentSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            paymentSettings.UpdatedDate = DateTime.Now;
            paymentSettings.UpdatedBy = _securityProvider.User.UserName;

            _eventBus.Publish(new PaymentSettingUpdated(paymentSettings));

            _repository.SaveChanges();
        }

        [Permission(Permissions.Activate, Module = Modules.PaymentSettings)]
        public void Enable(PaymentSettingsId id, string remarks)
        {
            var paymentSettings = _repository.PaymentSettings.Single(x => x.Id == id);
            paymentSettings.Enabled = true;
            paymentSettings.EnabledDate = DateTime.Now;
            paymentSettings.EnabledBy = _securityProvider.User.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingActivated(paymentSettings)
            {
                Remarks = remarks
            });
        }

        [Permission(Permissions.Deactivate, Module = Modules.PaymentSettings)]
        public void Disable(PaymentSettingsId id, string remarks)
        {
            var paymentSettings = _repository.PaymentSettings.Single(x => x.Id == id);
            paymentSettings.Enabled = false;
            paymentSettings.DisabledDate = DateTime.Now;
            paymentSettings.DisabledBy = _securityProvider.User.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new PaymentSettingDeactivated(paymentSettings)
            {
                Remarks = remarks
            });

        }
    }
}
