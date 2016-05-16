using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Data.Commands;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentLevelCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly PaymentQueries _paymentQueries;
        private readonly IBrandRepository _brandRepository;
        private readonly PlayerCommands _playerCommands;
        private readonly PaymentLevelQueries _paymentLevelQueries;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;
        private readonly BrandCommands _brandCommands;

        public PaymentLevelCommands(
            IPaymentRepository repository,
            PaymentQueries paymentQueries,
            IBrandRepository brandRepository,
            PlayerCommands playerCommands,
            ISecurityProvider securityProvider,
            IEventBus eventBus,
            PaymentLevelQueries paymentLevelQueries, BrandCommands brandCommands)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _brandRepository = brandRepository;
            _playerCommands = playerCommands;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
            _paymentLevelQueries = paymentLevelQueries;
            _brandCommands = brandCommands;
        }

        [Permission(Permissions.Add, Module = Modules.PaymentLevelManager)]
        public PaymentLevelSaveResult Save(EditPaymentLevel model)
        {
            Brand.Data.Brand brand = null;

            if (model.Brand.HasValue)
                brand = _brandRepository.Brands.Include(b => b.BrandCurrencies).SingleOrDefault(b => b.Id == model.Brand);

            ValidateCreatePaymentLevelModel(model, brand);

            var currency = brand.BrandCurrencies.Single(c => c.CurrencyCode == model.Currency);
            var currencyCode = currency.CurrencyCode;

            var bankAccounts = GetBankAccounts(model);

            var now = DateTimeOffset.UtcNow;

            var paymentLevel = new PaymentLevel
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                CurrencyCode = currencyCode,
                Status = PaymentLevelStatus.Active,
                CreatedBy = _securityProvider.User.UserName,
                DateCreated = now,
                ActivatedBy = _securityProvider.User.UserName,
                DateActivated = now,
                Code = model.Code,
                Name = model.Name,
                EnableOfflineDeposit = model.EnableOfflineDeposit
            };

            _repository.PaymentLevels.Add(paymentLevel);

            if (model.IsDefault)
                _brandCommands.MakePaymentLevelDefault(paymentLevel.Id, paymentLevel.BrandId, currencyCode);

            paymentLevel.BankAccounts = new List<BankAccount>();

            if (bankAccounts != null)
            {
                foreach (var bankAccount in bankAccounts.Where(x => x.Status == BankAccountStatus.Active))
                    paymentLevel.BankAccounts.Add(bankAccount);
            }

            _repository.SaveChanges();

            _eventBus.Publish(new PaymentLevelAdded(paymentLevel));

            return new PaymentLevelSaveResult
            {
                Message = "app:payment.levelCreated",
                PaymentLevelId = paymentLevel.Id
            };
        }

        private void ValidateCreatePaymentLevelModel(EditPaymentLevel model, Brand.Data.Brand brand)
        {
            if (brand == null)
                throw new RegoException("app:common.invalidBrand");

            var currency = brand.BrandCurrencies.SingleOrDefault(c => c.CurrencyCode == model.Currency);
            if (currency == null)
                throw new RegoException("app:payment.invalidCurrency");

            var paymentLevels = _paymentQueries.GetPaymentLevels();
            if (paymentLevels.Any(pl => pl.Name == model.Name && pl.BrandId == brand.Id && pl.Id != model.Id))
                throw new RegoException("app:payment.levelNameUnique");

            if (paymentLevels.Any(pl => pl.Code == model.Code && pl.BrandId == brand.Id && pl.Id != model.Id))
                throw new RegoException("app:payment.levelCodeUnique");

            if (model.IsDefault &&
                paymentLevels.Any(
                    pl => pl.Id != model.Id && pl.BrandId == model.Brand && pl.CurrencyCode == model.Currency && pl.IsDefault))
                throw new RegoException("Default payment level for the brand and currency combination already exists.");
        }

        [Permission(Permissions.Edit, Module = Modules.PaymentLevelManager)]
        public PaymentLevelSaveResult Edit(EditPaymentLevel model)
        {
            var id = model.Id;

            var paymentLevel = _paymentQueries.GetPaymentLevel(id.Value);
            if (paymentLevel == null)
                throw new RegoException("app:common.invalidId");

            var currencyCode = paymentLevel.CurrencyCode;
            Guid? brandId = paymentLevel.BrandId;

            var paymentLevels = _paymentQueries.GetPaymentLevels();
            if (paymentLevels.Any(pl => pl.Name == model.Name && pl.BrandId == brandId && pl.Id != model.Id))
                throw new RegoException("app:payment.levelNameUnique");

            if (paymentLevels.Any(pl => pl.Code == model.Code && pl.BrandId == brandId && pl.Id != model.Id))
                throw new RegoException("app:payment.levelCodeUnique");

            if (model.IsDefault && paymentLevels.Any(pl => pl.Id != model.Id && pl.BrandId == model.Brand && pl.CurrencyCode == model.Currency && pl.IsDefault))
                throw new RegoException("Default payment level for the brand and currency combination already exists.");

            var bankAccounts = GetBankAccounts(model);

            var now = DateTimeOffset.UtcNow;

            paymentLevel.UpdatedBy = _securityProvider.User.UserName;
            paymentLevel.DateUpdated = now;

            paymentLevel.Code = model.Code;
            paymentLevel.Name = model.Name;
            paymentLevel.EnableOfflineDeposit = model.EnableOfflineDeposit;

            if (model.IsDefault)
                _brandCommands.MakePaymentLevelDefault(paymentLevel.Id, paymentLevel.BrandId, currencyCode);

            paymentLevel.BankAccounts = new List<BankAccount>();
            if (bankAccounts != null)
            {
                foreach (var bankAccount in bankAccounts.Where(x => x.Status == BankAccountStatus.Active))
                    paymentLevel.BankAccounts.Add(bankAccount);
            }

            _repository.SaveChanges();
            _eventBus.Publish(new PaymentLevelEdited(paymentLevel));

            return new PaymentLevelSaveResult
            {
                Message = "app:payment.levelUpdated",
                PaymentLevelId = paymentLevel.Id
            };
        }

        private IQueryable<BankAccount> GetBankAccounts(EditPaymentLevel model)
        {
            IQueryable<BankAccount> bankAccounts = null;
            if (model.BankAccounts != null)
            {
                var bankAccountIds = model.BankAccounts.Select(ba => new Guid(ba));
                bankAccounts = _paymentQueries.GetBankAccounts().Where(ba => bankAccountIds.Contains(ba.Id));
                if (bankAccounts.Any(ba => ba.CurrencyCode != model.Currency))
                {
                    throw new RegoException("app:payment.levelAccountCurrencyMismatch");
                }
            }
            return bankAccounts;
        }

        public ValidationResult ValidatePaymentLevelCanBeActivated(ActivatePaymentLevelCommand command)
        {
            var validator = new ActivatePaymentLevelValidator(_repository);
            var validationResult = validator.Validate(command);
            return validationResult;
        }

        [Permission(Permissions.Activate, Module = Modules.PaymentLevelManager)]
        public void Activate(ActivatePaymentLevelCommand command)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var paymentLevel = _repository.PaymentLevels.Single(x => x.Id == command.Id);

                paymentLevel.Status = PaymentLevelStatus.Active;
                paymentLevel.ActivatedBy = _securityProvider.User.UserName;
                paymentLevel.DateActivated = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new PaymentLevelActivated(paymentLevel)
                {
                    Remarks = command.Remarks
                });

                scope.Complete();
            }
        }

        public ValidationResult ValidatePaymentLevelCanBeDeactivated(DeactivatePaymentLevelCommand command)
        {
            var validator = new DeactivatePaymentLevelValidator(_repository, _paymentLevelQueries);
            var validationResult = validator.Validate(command);
            return validationResult;
        }

        [Permission(Permissions.Deactivate, Module = Modules.PaymentLevelManager)]
        public void Deactivate(DeactivatePaymentLevelCommand command)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var oldPaymentLevel = _repository.PaymentLevels.Single(x => x.Id == command.Id);

                oldPaymentLevel.Status = PaymentLevelStatus.Inactive;
                oldPaymentLevel.DeactivatedBy = _securityProvider.User.UserName;
                oldPaymentLevel.DateDeactivated = DateTimeOffset.UtcNow;

                if (command.NewPaymentLevelId.HasValue)
                {
                    _playerCommands.UpdatePlayersPaymentLevel(command.Id, command.NewPaymentLevelId.Value);

                    var newPaymentLevel = _repository.PaymentLevels.Single(x => x.Id == command.NewPaymentLevelId);

                    _brandCommands.MakePaymentLevelDefault(newPaymentLevel.Id, newPaymentLevel.BrandId, newPaymentLevel.CurrencyCode);

                    _repository.PlayerPaymentLevels
                        .Include(x => x.PaymentLevel)
                        .Where(x => x.PaymentLevel.Id == command.Id)
                        .ForEach(x => x.PaymentLevel = newPaymentLevel);
                }

                _repository.SaveChanges();

                _eventBus.Publish(new PaymentLevelDeactivated(oldPaymentLevel)
                {
                    Remarks = command.Remarks
                });

                scope.Complete();
            }
        }
    }
}
