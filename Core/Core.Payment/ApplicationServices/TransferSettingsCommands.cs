using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Domain.Payment.ApplicationServices
{
    public class TransferSettingsCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;

        public TransferSettingsCommands(
            IPaymentRepository repository,
            IEventBus eventBus,
            ISecurityProvider securityProvider)
        {
            _repository = repository;
            _eventBus = eventBus;
            _securityProvider = securityProvider;
        }

        [Permission(Permissions.Add, Module = Modules.TransferSettings)]
        public Guid AddSettings(SaveTransferSettingsCommand model)
        {
            var validationResult = new SaveTransferSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            if (_repository.TransferSettings.Any(x => x.Id == model.Id))
            {
                throw new RegoException(TransferFundSettingsErrors.AlreadyExistsError.ToString());
            }

            if (_repository.TransferSettings.Any(
                x => x.BrandId == model.Brand
                    && x.VipLevelId == model.VipLevel
                    && x.TransferType == model.TransferType
                    && x.CurrencyCode == model.Currency
                    && x.WalletId == model.Wallet))
            {
                throw new RegoException(TransferFundSettingsErrors.AlreadyExistsError.ToString());
            }

            var transferSettings = new TransferSettings();
            transferSettings.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            transferSettings.BrandId = model.Brand;
            transferSettings.CurrencyCode = model.Currency;
            transferSettings.VipLevelId = model.VipLevel;
            transferSettings.TransferType = model.TransferType;
            transferSettings.WalletId = model.Wallet;
            transferSettings.CreatedDate = DateTimeOffset.Now.ToBrandOffset(model.TimezoneId);
            transferSettings.CreatedBy = _securityProvider.User.UserName;
            transferSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            transferSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            transferSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            transferSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            transferSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            transferSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            _repository.TransferSettings.Add(transferSettings);
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsCreated(transferSettings));

            return transferSettings.Id;
        }

        [Permission(Permissions.Edit, Module = Modules.TransferSettings)]
        public void UpdateSettings(SaveTransferSettingsCommand model)
        {
            var validationResult = new SaveTransferSettingsValidator().Validate(model);
            if (!validationResult.IsValid)
            {
                throw new RegoException(validationResult.Errors.First().ErrorMessage);
            }

            var transferSettings = _repository.TransferSettings.SingleOrDefault(x => x.Id == model.Id);
            if (transferSettings == null)
                throw new RegoException(string.Format("Unable to find Transfer Settings with Id '{0}'", model.Id));

            transferSettings.MinAmountPerTransaction = model.MinAmountPerTransaction;
            transferSettings.MaxAmountPerTransaction = model.MaxAmountPerTransaction;
            transferSettings.MaxAmountPerDay = model.MaxAmountPerDay;
            transferSettings.MaxTransactionPerDay = model.MaxTransactionPerDay;
            transferSettings.MaxTransactionPerWeek = model.MaxTransactionPerWeek;
            transferSettings.MaxTransactionPerMonth = model.MaxTransactionPerMonth;
            transferSettings.UpdatedDate = DateTimeOffset.Now.ToBrandOffset(model.TimezoneId);
            transferSettings.UpdatedBy = _securityProvider.User.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsUpdated(transferSettings));
        }

        [Permission(Permissions.Activate, Module = Modules.TransferSettings)]
        public void Enable(TransferSettingsId id, string timezoneId, string remarks)
        {
            var transferSettings = _repository.TransferSettings.Single(x => x.Id == id);
            transferSettings.Enabled = true;
            transferSettings.EnabledDate = DateTimeOffset.Now.ToBrandOffset(timezoneId);
            transferSettings.EnabledBy = _securityProvider.User.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsActivated(transferSettings)
            {
                Remarks = remarks
            });
        }

        [Permission(Permissions.Deactivate, Module = Modules.TransferSettings)]
        public void Disable(TransferSettingsId id, string timezoneId, string remarks)
        {
            var transferSettings = _repository.TransferSettings.Single(x => x.Id == id);
            transferSettings.Enabled = false;
            transferSettings.DisabledDate = DateTimeOffset.Now.ToBrandOffset(timezoneId);
            transferSettings.DisabledBy = _securityProvider.User.UserName;
            _repository.SaveChanges();

            _eventBus.Publish(new TransferFundSettingsDeactivated(transferSettings)
            {
                Remarks = remarks
            });
        }
    }
}