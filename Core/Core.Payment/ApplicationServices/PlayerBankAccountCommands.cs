using System;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Services.Payment.Validators;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class PlayerBankAccountCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _queries;
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        public PlayerBankAccountCommands(
            IPaymentRepository repository, 
            IPaymentQueries queries,
            ISecurityProvider securityProvider, 
            IEventBus eventBus)
        {
            _repository = repository;            
            _queries = queries;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Add, Module = Modules.PlayerBankAccount)]
        public PlayerBankAccount Add(EditPlayerBankAccountCommand model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                var validationResult = new AddPlayerBankAccountValidator(_repository, _queries).Validate(model);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                var player = _repository.Players
                    .Include(x => x.CurrentBankAccount)
                    .Single(x => x.Id == model.PlayerId);

                var bank = _repository.Banks.Single(x => x.Id == model.Bank);

                var bankAccount = new PlayerBankAccount
                {
                    Id = Guid.NewGuid(),
                    Player = player,
                    Status = BankAccountStatus.Pending,
                    Bank = bank,
                    Province = model.Province,
                    City = model.City,
                    Branch = model.Branch,
                    SwiftCode = model.SwiftCode,
                    Address = model.Address,
                    AccountName = model.AccountName,
                    AccountNumber = model.AccountNumber,
                    Created = DateTimeOffset.UtcNow,
                    CreatedBy = _securityProvider.User.UserName
                };

                if (player.CurrentBankAccount == null)
                {
                    player.CurrentBankAccount = bankAccount;
                    bankAccount.IsCurrent = true;
                }                    

                _repository.PlayerBankAccounts.Add(bankAccount);
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountAdded(bankAccount));

                scope.Complete();
                return bankAccount;
            }
        }

        [Permission(Permissions.Edit, Module = Modules.PlayerBankAccount)]
        public void Edit(EditPlayerBankAccountCommand model)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope(IsolationLevel.RepeatableRead))
            {
                var validationResult = new EditPlayerBankAccountValidator(_repository, _queries).Validate(model);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                var bank = _repository.Banks.Single(x => x.Id == model.Bank);

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player.CurrentBankAccount)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == model.Id.Value);

                var isModified =
                    bankAccount.Bank.Id != bank.Id ||
                    bankAccount.Province != model.Province ||
                    bankAccount.City != model.City ||
                    bankAccount.Branch != model.Branch ||
                    bankAccount.SwiftCode != model.SwiftCode ||
                    bankAccount.Address != model.Address ||
                    bankAccount.AccountName != model.AccountName ||
                    bankAccount.AccountNumber != model.AccountNumber;

                if (isModified)
                {
                    bankAccount.Status = BankAccountStatus.Pending;
                }

                bankAccount.Bank = bank;
                bankAccount.Province = model.Province;
                bankAccount.City = model.City;
                bankAccount.Branch = model.Branch;
                bankAccount.SwiftCode = model.SwiftCode;
                bankAccount.Address = model.Address;
                bankAccount.AccountName = model.AccountName;
                bankAccount.AccountNumber = model.AccountNumber;
                bankAccount.Updated = DateTimeOffset.UtcNow;
                bankAccount.UpdatedBy = _securityProvider.User.UserName;

                _repository.SaveChanges();
                _eventBus.Publish(new PlayerBankAccountEdited(bankAccount));
                scope.Complete();
            }
        }

        [Permission(Permissions.Edit, Module = Modules.PlayerBankAccount)]
        public void SetCurrent(PlayerBankAccountId playerBankAccountId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var setCurrentPlayerBankAccountCommand = new SetCurrentPlayerBankAccountCommand
                {
                    PlayerBankAccountId = playerBankAccountId
                };

                var validationResult = new SetCurrentPlayerBankAccountValidator(_repository).Validate(setCurrentPlayerBankAccountCommand);

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player.CurrentBankAccount)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == playerBankAccountId);

                bankAccount.Player.CurrentBankAccount.IsCurrent = false;
                bankAccount.Player.CurrentBankAccount = bankAccount;
                bankAccount.IsCurrent = true;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountCurrentSet(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber));

                scope.Complete();
            }
        }

        [Permission(Permissions.Verify, Module = Modules.PlayerBankAccount)]
        public void Verify(PlayerBankAccountId playerBankAccountId, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult =
                    new VerifyPlayerBankAccountValidator(_repository).Validate(new VerifyPlayerBankAccountData
                    {
                        Id = playerBankAccountId
                    });

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == playerBankAccountId);

                bankAccount.Status = BankAccountStatus.Verified;
                bankAccount.Remarks = remarks;
                bankAccount.VerifiedBy = _securityProvider.User.UserName;
                bankAccount.Verified = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountVerified(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber,
                    bankAccount.VerifiedBy,
                    bankAccount.Verified.Value,
                    bankAccount.Created,
                    bankAccount.Remarks));

                scope.Complete();
            }
        }

        [Permission(Permissions.Reject, Module = Modules.PlayerBankAccount)]
        public void Reject(PlayerBankAccountId playerBankAccountId, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var validationResult =
                    new RejectPlayerBankAccountValidator(_repository).Validate(new RejectPlayerBankAccountData
                    {
                        Id = playerBankAccountId
                    });

                if (!validationResult.IsValid)
                {
                    throw new RegoException(validationResult.Errors.First().ErrorMessage);
                }

                var bankAccount = _repository.PlayerBankAccounts
                    .Include(x => x.Player)
                    .Include(x => x.Bank)
                    .Single(x => x.Id == playerBankAccountId);

                bankAccount.Status = BankAccountStatus.Rejected;
                bankAccount.Remarks = remarks;
                bankAccount.RejectedBy = _securityProvider.User.UserName;
                bankAccount.Rejected = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerBankAccountRejected(
                    bankAccount.Player.Id,
                    bankAccount.Id,
                    bankAccount.AccountNumber,
                    bankAccount.RejectedBy,
                    bankAccount.Rejected.Value,
                    bankAccount.Created,
                    bankAccount.Remarks));

                scope.Complete();
            }
        }
    }
}
