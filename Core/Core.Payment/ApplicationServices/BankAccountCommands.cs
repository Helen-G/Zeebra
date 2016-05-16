using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankAccountCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IBrandRepository _brandRepository;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        public BankAccountCommands(
            IPaymentRepository repository, 
            IBrandRepository brandRepository, 
            ISecurityProvider securityProvider,
            IEventBus eventBus)
        {
            _repository = repository;
            _brandRepository = brandRepository;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Add, Module = Modules.BankAccounts)]
        public BankAccount Add(AddBankAccountData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new AddBankAccountValidator(_repository, _brandRepository).Validate(data);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                    //throw validationResult.GetValidationError();
                }

                var bankAccount = new BankAccount
                {
                    Id = Guid.NewGuid(),
                    Bank = _repository.Banks.Single(x => x.Id == data.Bank),
                    CurrencyCode = data.Currency,
                    AccountId = data.AccountId,
                    AccountName = data.AccountName,
                    AccountNumber = data.AccountNumber,
                    AccountType = data.AccountType,
                    Province = data.Province,
                    Branch = data.Branch,
                    Remarks = data.Remarks,
                    Status = BankAccountStatus.Pending,
                    Created = DateTime.UtcNow,
                    CreatedBy = _securityProvider.User.UserName
                };

                _repository.BankAccounts.Add(bankAccount);
                _repository.SaveChanges();

                _eventBus.Publish(new BankAccountAdded(bankAccount));

                scope.Complete();

                return bankAccount;
            }
        }

        [Permission(Permissions.Edit, Module = Modules.BankAccounts)]
        public void Edit(EditBankAccountData data)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult = new EditBankAccountValidator(_repository, _brandRepository).Validate(data);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                    //throw validationResult.GetValidationError();
                }

                var bankAccount = _repository.BankAccounts.Single(x => x.Id == data.Id);

                bankAccount.Bank = _repository.Banks.Single(x => x.Id == data.Bank);
                bankAccount.CurrencyCode = data.Currency;
                bankAccount.AccountId = data.AccountId;
                bankAccount.AccountName = data.AccountName;
                bankAccount.AccountNumber = data.AccountNumber;
                bankAccount.AccountType = data.AccountType;
                bankAccount.Province = data.Province;
                bankAccount.Branch = data.Branch;
                bankAccount.Remarks = data.Remarks;
                bankAccount.Updated = DateTime.UtcNow;
                bankAccount.UpdatedBy = _securityProvider.User.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new BankAccountEdited(bankAccount));

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Module = Modules.BankAccounts)]
        public void Activate(BankAccountId bankAccountId, string remarks)
        {
            var bankAccount = _repository.GetBankAccount(bankAccountId);
            bankAccount.Activate(_securityProvider.User.UserName, remarks);
            _repository.SaveChanges();
            var account = _repository.BankAccounts.Single(x => x.Id == bankAccountId);
            _eventBus.Publish(new BankAccountActivated(account));
        }

        [Permission(Permissions.Deactivate, Module = Modules.BankAccounts)]
        public void Deactivate(BankAccountId bankAccountId, string remarks)
        {
            var bankAccount = _repository.GetBankAccount(bankAccountId);
            bankAccount.Deactivate(_securityProvider.User.UserName, remarks);
            _repository.SaveChanges();
            var account = _repository.BankAccounts.Single(x => x.Id == bankAccountId);
            _eventBus.Publish(new BankAccountDeactivated(account));
        }
    }
}
