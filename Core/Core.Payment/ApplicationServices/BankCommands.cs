using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        public BankCommands(IPaymentRepository repository, ISecurityProvider securityProvider, IEventBus eventBus)
        {
            _repository = repository;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Edit, Module = Modules.Banks)]
        public SaveResult Edit(SaveBankData command)
        {
            var brand = ValidateBrandSaveCommand(command);

            var bank = _repository.Banks
                .Single(x => x.Id == command.Id);

            bank.Updated = DateTime.Now;
            bank.UpdatedBy = _securityProvider.User.UserName;
            bank.BankId = command.BankId;
            bank.Name = command.BankName;
            bank.CountryCode = command.Country;
            bank.BrandId = brand.Id;
            bank.Remark = command.Remark;

            _repository.SaveChanges();
            _eventBus.Publish(new BankEdited(bank));

            return new SaveResult
            {
                Id = bank.Id,
                Message = "app:banks.updated"
            };
        }

        [Permission(Permissions.Add, Module = Modules.Banks)]
        public SaveResult Save(SaveBankData command)
        {
            var brand = ValidateBrandSaveCommand(command);

            var bank = new Bank
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                CreatedBy = _securityProvider.User.UserName,
                BankId = command.BankId,
                Name = command.BankName,
                CountryCode = command.Country,
                BrandId = brand.Id,
                Remark = command.Remark
            };

            _repository.Banks.Add(bank);
            _repository.SaveChanges();
            _eventBus.Publish(new BankAdded(bank));

            return new SaveResult
            {
                Id = bank.Id,
                Message = "app:banks.created"
            };
        }

        private RegoV2.Domain.Payment.Data.Brand ValidateBrandSaveCommand(SaveBankData command)
        {
            var brand = _repository.Brands.FirstOrDefault(x => x.Id == command.Brand);

            if (brand == null)
            {
                throw new RegoException("app:common.invalidBrand");
            }

            if (_repository.Banks.Any(x =>
                (command.Id == Guid.Empty || command.Id != x.Id) &&
                x.Brand.Id == brand.Id &&
                x.BankId == command.BankId))
            {
                throw new RegoException("app:banks.bankIdUnique");
            }

            if (_repository.Banks.Any(x =>
                (command.Id == Guid.Empty || command.Id != x.Id) &&
                x.Brand.Id == brand.Id &&
                x.Name == command.BankName))
            {
                throw new RegoException("app:banks.bankNameUnique");
            }
            return brand;
        }

        public void AddBank(Bank bank)
        {
            _repository.Banks.Add(bank);
            _repository.SaveChanges();
        }
    }

    public class SaveResult
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }

    public class SaveBankData
    {
        public Guid Id { get; set; }
        public Guid Brand { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string Country { get; set; }
        public string Remark { get; set; }
    }
}
