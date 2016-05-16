using System;
using System.IO;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class OfflineDepositCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IFileStorage _fileStarage;
        private readonly IEventBus _eventBus;
        private readonly IWalletCommands _walletCommands;
        private readonly IOfflineDepositValidator _validator;
        private readonly ISecurityProvider _securityProvider;
        private readonly IPlayerIdentityValidator _identityValidator;

        public OfflineDepositCommands(
            IPaymentRepository repository,
            IPaymentQueries paymentQueries,
            IFileStorage fileStarage,
            IEventBus eventBus,
            IWalletCommands walletCommands,
            IOfflineDepositValidator validator,
            ISecurityProvider securityProvider,
            IPlayerIdentityValidator identityValidator)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _fileStarage = fileStarage;
            _eventBus = eventBus;
            _walletCommands = walletCommands;
            _validator = validator;
            _securityProvider = securityProvider;
            _identityValidator = identityValidator;
        }

        // KB: Moved permission attribute here to allow member API to call Submit method.
        // Method should be deleted after bug with calling Submit from API is fixed
        [Permission(Permissions.Add, Module = Modules.OfflineDepositRequests)]
        public void NeedToRework() { }

        public OfflineDeposit Submit(OfflineDepositRequest request)
        {
            var player = _paymentQueries.GetPlayer(request.PlayerId);
            var bankAccount = _paymentQueries.GetBankAccount(request.BankAccountId);

            if (bankAccount == null || bankAccount.Status != BankAccountStatus.Active)
                throw new ArgumentException("app:payment.deposit.bankAccountNotFound");

            if (bankAccount.CurrencyCode != player.CurrencyCode)
                throw new ArgumentException("app:payment.deposit.differentCurrenciesErrorMessage");

            _validator.ValidatePaymentSetting(request.PlayerId, request.BankAccountId, request.Amount);
            _identityValidator.Validate(request.PlayerId, TransactionType.Deposit);

            var id = Guid.NewGuid();
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var number = GenerateTransactionNumber();

                var offlineDeposit = _repository.CreateOfflineDeposit(id, number, request, bankAccount.Id, player);

                var depositEvent = offlineDeposit.Submit();
                _repository.SaveChanges();

                _eventBus.Publish(depositEvent);

                scope.Complete();
            }

            return _paymentQueries.GetDepositById(id);
        }

        [Permission(Permissions.Confirm, Module = Modules.OfflineDepositConfirmation)]
        public OfflineDeposit Confirm(
            OfflineDepositConfirm depositConfirm,
            byte[] idFrontImage,
            byte[] idBackImage,
            byte[] receiptImage)
        {
            var offlineDeposit = _repository.GetDepositById(depositConfirm.Id);

            var idFrontImageName = SaveFile(depositConfirm.IdFrontImage, String.Format("{0}-FrontId", depositConfirm.Id),
                idFrontImage);
            var idBackImageName = SaveFile(depositConfirm.IdBackImage, String.Format("{0}-BackId", depositConfirm.Id),
                idBackImage);
            var receiptImageName = SaveFile(depositConfirm.ReceiptImage, String.Format("{0}-ReceiptId", depositConfirm.Id),
                receiptImage);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                ValidateLimits(depositConfirm);
                var confirmEvent = offlineDeposit.Confirm(
                    depositConfirm.PlayerAccountName,
                    depositConfirm.PlayerAccountNumber,
                    depositConfirm.ReferenceNumber,
                    depositConfirm.Amount,
                    depositConfirm.TransferType,
                    depositConfirm.OfflineDepositType,
                    depositConfirm.Remark,
                    idFrontImageName,
                    idBackImageName,
                    receiptImageName);

                _repository.SaveChanges();

                _eventBus.Publish(confirmEvent);
                
                scope.Complete();
            }

            return _paymentQueries.GetDepositById(depositConfirm.Id);
        }

        private void ValidateLimits(OfflineDepositConfirm depositConfirm)
        {
            var deposit = _paymentQueries.GetDepositById(depositConfirm.Id);
            var playerId = deposit.PlayerId;
            var amount = depositConfirm.Amount;
            var bankAccountId = deposit.BankAccountId;

            _validator.ValidatePaymentSetting(playerId, bankAccountId, amount);
        }

        [Permission(Permissions.Verify, Module = Modules.OfflineDepositVerification)]
        public void Verify(OfflineDepositId id, string remark)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var verifyEvent = offlineDeposit.Verify(_securityProvider.User.UserName, remark);
                
                _repository.SaveChanges();

                _eventBus.Publish(verifyEvent);

                scope.Complete();
            }
        }

        [Permission(Permissions.Unverify, Module = Modules.OfflineDepositVerification)]
        public void Unverify(OfflineDepositId id, string remark, UnverifyReasons unverifyReason)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var unverifyEvent = offlineDeposit.Unverify(_securityProvider.User.UserName, remark, unverifyReason);
                _repository.SaveChanges();

                _eventBus.Publish(unverifyEvent);

                scope.Complete();
            }
        }

        [Permission(Permissions.Approve, Module = Modules.OfflineDepositApproval)]
        public void Approve(OfflineDepositApprove approveCommand)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineDeposit = _repository.GetDepositById(approveCommand.Id);
                _validator.ValidatePaymentSetting(offlineDeposit.Data.PlayerId,
                    offlineDeposit.Data.BankAccountId,
                    approveCommand.ActualAmount);

                var depositApproved = offlineDeposit.Approve(
                    approveCommand.ActualAmount,
                    approveCommand.Fee,
                    approveCommand.PlayerRemark,
                    _securityProvider.User.UserName,
                    approveCommand.Remark);

                _walletCommands.Deposit(depositApproved.PlayerId, depositApproved.ActualAmount, offlineDeposit.Data.TransactionNumber);
                _repository.SaveChanges();

                _eventBus.Publish(depositApproved);

                scope.Complete();
            }
        }

        [Permission(Permissions.Reject, Module = Modules.OfflineDepositApproval)]
        public void Reject(OfflineDepositId id, string remark)
        {
            var offlineDeposit = _repository.GetDepositById(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var rejectedEvent = offlineDeposit.Reject(_securityProvider.User.UserName, remark);
                _repository.SaveChanges();

                _eventBus.Publish(rejectedEvent);

                scope.Complete();
            }
        }

        private string SaveFile(string fileName, string fileNameTemplate, byte[] content)
        {
            if (content != null && content.Length > 0)
            {
                string format = String.Format("{0}.xxx", fileNameTemplate);
                var newFileName = Path.ChangeExtension(format, Path.GetExtension(fileName));
                _fileStarage.Save(newFileName, content);
                return newFileName;
            }
            return null;
        }

        private static string GenerateTransactionNumber()
        {
            var random = new Random();
            return "OD" + random.Next(10000000, 99999999);
        }
    }
}