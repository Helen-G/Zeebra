using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events;
using AFT.RegoV2.Domain.BoundedContexts.Payment.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class WithdrawalService : MarshalByRefObject, IApplicationService
    {
        private readonly BrandQueries _brandQueries;
        private readonly WalletQueries _walletQueries;
        private readonly IPaymentRepository _repository;
        private readonly IPaymentQueries _paymentQueries;
        private readonly IWalletCommands _walletCommands;
        private readonly IOfflineWithdrawalValidationService _offlineWithdrawalValidationService;
        private readonly IServiceBus _serviceBus;
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;

        public WithdrawalService(
            IPaymentRepository repository,
            IPaymentQueries paymentQueries,
            IServiceBus serviceBus,
            IEventBus eventBus,
            WalletQueries walletQueries,
            IWalletCommands walletCommands,
            BrandQueries brandQueries,
            IOfflineWithdrawalValidationService offlineWithdrawalValidationService,
            ISecurityProvider securityProvider)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _serviceBus = serviceBus;
            _eventBus = eventBus;
            _walletQueries = walletQueries;
            _walletCommands = walletCommands;
            _brandQueries = brandQueries;
            _offlineWithdrawalValidationService = offlineWithdrawalValidationService;
            _securityProvider = securityProvider;
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalRequest)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForVerification()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Pending || x.Status == WithdrawalStatus.Reverted);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalVerification)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForAcceptance()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Verified);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalAcceptance)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsForApproval()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Accepted);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalAcceptance)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsCanceled()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.Canceled);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalWagerCheck)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsFailedAutoWagerCheck()
        {
            return GetWithdrawals().Where(x => !x.AutoWagerCheck && x.Status == WithdrawalStatus.Pending);
        }

        [Permission(Permissions.View, Module = Modules.OfflineWithdrawalOnHold)]
        public IQueryable<OfflineWithdraw> GetWithdrawalsOnHold()
        {
            return GetWithdrawals().Where(x => x.Status == WithdrawalStatus.OnHold);
        }

        public IQueryable<OfflineWithdraw> GetWithdrawals()
        {
            return _repository.OfflineWithdraws.AsQueryable()
                .Include(p => p.PlayerBankAccount.Player)
                .Include(p => p.PlayerBankAccount.Bank.Brand);
        }

        public class BankAccountModifiedException : Exception
        {
        }

        public class BankModifedException : Exception
        {
        }

        [Permission(Permissions.Add, Module = Modules.OfflineWithdrawalRequest)]
        public OfflineWithdrawResponse Request(OfflineWithdrawRequest request)
        {
            return ProcessWithdrawal(request);
        }

        public OfflineWithdrawResponse ProcessWithdrawal(OfflineWithdrawRequest request)
        {
            var offlineWithdrawResponse = new OfflineWithdrawResponse();

            // Consider to validate whether the bank account has changed since the user view the account information in the form and now that we are actually saving.

            var bankAccount =
                _repository.PlayerBankAccounts.Include(x => x.Player)
                    .Include(x => x.Bank)
                    .SingleOrDefault(x => x.Id == request.PlayerBankAccountId);

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }

            if (bankAccount == null)
            {
                throw new ArgumentException("Player does not have a current and verified bank account.");
            }

            var userBankAccountTime = DateTimeOffset.Parse(request.BankAccountTime);
            var storedBankAccountTime = bankAccount.Updated ?? bankAccount.Created;
            if (userBankAccountTime.ToString() != storedBankAccountTime.ToString())
            {
                throw new BankAccountModifiedException();
            }

//            var userBankTime = DateTimeOffset.Parse(request.BankTime);
//            var storedBankTime = bankAccount.Bank.Updated ?? bankAccount.Bank.Created;
//            if (userBankTime.ToString() != storedBankTime.ToString())
//            {
//                offlineWithdrawResponse.Exceptions.Add(new BankModifedException());
//                return offlineWithdrawResponse;
//            }

            _offlineWithdrawalValidationService.Validate(request);

            var id = offlineWithdrawResponse.Id = Guid.NewGuid();
            var withdrawal = new OfflineWithdraw();
            bankAccount.EditLock = true;

            _repository.OfflineWithdraws.Add(withdrawal);
            var number = GenerateTransactionNumber();
            withdrawal.Id = id;
            withdrawal.PlayerBankAccount = bankAccount;
            withdrawal.TransactionNumber = number;
            withdrawal.Amount = request.Amount;
            withdrawal.CreatedBy = request.RequestedBy;
            withdrawal.Created = DateTimeOffset.Now;
            withdrawal.Remarks = request.Remarks;
            withdrawal.Status = WithdrawalStatus.Pending;

//                var now = DateTimeOffset.Now;
//                var player = bankAccount.Player;
//                var depositCount = _repository.OfflineDeposits.Count(x => x.PlayerId == player.Id && x.Status == OfflineDepositStatus.Approved);
//                var amount = _repository.OfflineDeposits.Where(x => x.PlayerId == player.Id && x.Status == OfflineDepositStatus.Approved).Sum(x => (decimal?)x.ActualAmount) ?? 0;
//                var withdrawalCount = _repository.OfflineWithdraws.Count(x => x.PlayerBankAccount.Player.Id == player.Id && x.Status == WithdrawalStatus.Approved);
//                var bets = _gameRepository.GetPlayerBets(player.Id);
//                var lostAmount = bets.Sum(x => x.Data.LostAmount);
//                var accountAge = now - player.DateRegistered;

            // TODO These rules should be configurable in fraud system.
//                var passAutoVerify = depositCount > 10;
//                passAutoVerify = passAutoVerify && amount > 100000;
//                passAutoVerify = passAutoVerify && withdrawalCount > 10;
//                passAutoVerify = passAutoVerify && lostAmount < -100000;
//                passAutoVerify = passAutoVerify && accountAge > TimeSpan.FromDays(90);
//                withdrawal.AutoVerify = passAutoVerify;
//                withdrawal.AutoVerifyTime = DateTimeOffset.Now;
//
//                if (!passAutoVerify)
//                {
//                    if (player.ExemptWithdrawalVerification.HasValue && player.ExemptWithdrawalVerification.Value)
//                    {
//                        if (now >= player.ExemptWithdrawalFrom && now <= player.ExemptWithdrawalTo)
//                        {
//                            var count =
//                                _repository.OfflineWithdraws.Count(x => x.PlayerBankAccount.Player.Id == player.Id && x.Exempted &&
//                                    x.ExemptionCheckTime.Value >= player.ExemptWithdrawalFrom && x.ExemptionCheckTime.Value <= player.ExemptWithdrawalTo);
//                            if (count < player.ExemptLimit)
//                            {
//                                withdrawal.Exempted = true;
//                            }
//                        }
//                    }
//                    withdrawal.ExemptionCheckTime = now;
//                }


            withdrawal.AutoWagerCheck = _walletQueries.GetWageringLockedBalanceOfPlayer(bankAccount.Player.Id) == 0;
            withdrawal.AutoWagerCheckTime = DateTimeOffset.Now;
            AppendWagerCheckComments(withdrawal, "System");

            _eventBus.Publish(new WithdrawalCreated
            {
                CreatedBy = withdrawal.CreatedBy,
                Created = withdrawal.Created,
                PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                TransactionNumber = withdrawal.TransactionNumber,
                Amount = withdrawal.Amount,
            });

            if (request.NotificationType == NotificationType.Sms)
                _serviceBus.PublishMessage(new SmsCommandMessage(bankAccount.Player.PhoneNumber,
                    "Offline withdrawal request created. Amount: " + withdrawal.Amount));
            if (request.NotificationType == NotificationType.Email)
                _serviceBus.PublishMessage(new EmailCommandMessage(
                    "support@flycowgames.com",
                    "FlyCow Games",
                    bankAccount.Player.Email,
                    bankAccount.Player.Username,
                    "Offline withdrawal request",
                    "Offline withdrawal request created. Amount: " + withdrawal.Amount));

            // TODO Add QP check. Wager Check may be changed or removed.
            if (( /*passAutoVerify || */withdrawal.Exempted) && withdrawal.AutoWagerCheck)
            {
                withdrawal.Status = WithdrawalStatus.Verified;
                withdrawal.VerifiedBy = "System";
                withdrawal.Verified = DateTimeOffset.Now;
                _eventBus.Publish(new WithdrawalVerified(withdrawal));
            }

            _repository.SaveChanges();


            return offlineWithdrawResponse;
        }

        public void AppendWagerCheckComments(OfflineWithdraw offlineWithdraw, string checker)
        {
            var textToAppend = "Wager Checked by: " + checker + "\nDate Wager Checked: " + DateTime.Now.ToString("MM'/'dd'/'yyyy HH:mm");
            var remarks = offlineWithdraw.Remarks ?? "";
            var match = Regex.Match(remarks, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);
            if (match.Success)
            {
                if (match.Value.Trim().Length != 0)
                {
                    textToAppend = "\n\n" + textToAppend;
                }
                else
                {
                    match = match.NextMatch();
                    if (match.Success && match.Value.Trim().Length != 0)
                    {
                        textToAppend = "\n" + textToAppend;
                    }
                }
            }
            // potential string length issue
            offlineWithdraw.Remarks = remarks + textToAppend;
        }

        [Permission(Permissions.Verify, Module = Modules.OfflineWithdrawalVerification)]
        public OfflineWithdraw Verify(OfflineWithdrawId id, string remarks)
        {
            OfflineWithdraw offlineWithdrawal;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                offlineWithdrawal = GetWithdrawalWithPlayer(id);
                if (offlineWithdrawal.Status != WithdrawalStatus.Pending && offlineWithdrawal.Status != WithdrawalStatus.Reverted)
                {
                    ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Verified);
                }
                offlineWithdrawal.Verified = DateTimeOffset.Now;
                offlineWithdrawal.VerifiedBy = _securityProvider.User.UserName;
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Verified;
                _eventBus.Publish(new WithdrawalVerified(offlineWithdrawal));
                _repository.SaveChanges();

                scope.Complete();
            }
            return offlineWithdrawal;
        }

        private void ThrowStatusError(WithdrawalStatus from, WithdrawalStatus to)
        {
            throw new InvalidOperationException(string.Format("The withdrawal has \"{0}\" status, so it can't be {1}", from, to));
        }

        [Permission(Permissions.Unverify, Module = Modules.OfflineWithdrawalVerification)]
        public OfflineWithdraw Unverify(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = _repository.OfflineWithdraws.Include(x => x.PlayerBankAccount.Player).Single(x => x.Id == id);
                if (withdrawal.Status != WithdrawalStatus.Pending && withdrawal.Status != WithdrawalStatus.Reverted)
                {
                    ThrowStatusError(withdrawal.Status, WithdrawalStatus.Unverified);
                }
                withdrawal.Unverified = DateTimeOffset.Now;
                withdrawal.UnverifiedBy = _securityProvider.User.UserName;
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Unverified;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();

                scope.Complete();

                return withdrawal;
            }
        }

        [Permission(Permissions.Approve, Module = Modules.OfflineWithdrawalApproval)]
        public OfflineWithdraw Approve(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                if (offlineWithdrawal.Status != WithdrawalStatus.Accepted)
                {
                    ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Approved);
                }
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Approved;
                offlineWithdrawal.Approved = DateTimeOffset.Now;
                offlineWithdrawal.ApprovedBy = _securityProvider.User.UserName;
                _walletCommands.Withdraw(offlineWithdrawal.PlayerBankAccount.Player.Id, offlineWithdrawal.Amount, offlineWithdrawal.TransactionNumber);
                _eventBus.Publish(new WithdrawalApproved(offlineWithdrawal));
                _repository.SaveChanges();
                scope.Complete();
                return offlineWithdrawal;
            }
        }

        [Permission(Permissions.Reject, Module = Modules.OfflineWithdrawalApproval)]
        public OfflineWithdraw Reject(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                if (withdrawal.Status != WithdrawalStatus.Accepted)
                {
                    ThrowStatusError(withdrawal.Status, WithdrawalStatus.Rejected);
                }
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Rejected;
                withdrawal.Rejected = DateTimeOffset.Now;
                withdrawal.RejectedBy = _securityProvider.User.UserName;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();
                scope.Complete();
                return withdrawal;
            }
        }

        [Permission(Permissions.Pass, Module = Modules.OfflineWithdrawalWagerCheck)]
        public OfflineWithdraw PassWager(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                offlineWithdrawal.WagerCheck = true;
                offlineWithdrawal.Remarks = remarks;
                AppendWagerCheckComments(offlineWithdrawal, _securityProvider.User.UserName);
                _eventBus.Publish(new WithdrawalWagerChecked(offlineWithdrawal));
                _repository.SaveChanges();
                scope.Complete();
                return offlineWithdrawal;
            }
        }

        private OfflineWithdraw GetWithdrawalWithPlayer(Guid id)
        {
            return _repository.OfflineWithdraws.Include(x => x.PlayerBankAccount.Player).Single(x => x.Id == id);
        }

        [Permission(Permissions.Fail, Module = Modules.OfflineWithdrawalWagerCheck)]
        public OfflineWithdraw FailWager(OfflineWithdrawId id, string remarks)
        {
            var withdrawal = GetWithdrawalWithPlayer(id);
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                withdrawal.WagerCheck = false;
                withdrawal.Remarks = remarks;
                AppendWagerCheckComments(withdrawal, _securityProvider.User.UserName);
                withdrawal.Status = WithdrawalStatus.Unverified;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();
                scope.Complete();
                return withdrawal;
            }
        }

        [Permission(Permissions.Pass, Module = Modules.OfflineWithdrawalInvestigation)]
        public OfflineWithdraw PassInvestigation(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var offlineWithdrawal = GetWithdrawalWithPlayer(id);
                offlineWithdrawal.InvestigatedBy = _securityProvider.User.UserName;
                offlineWithdrawal.InvestigatedDate = DateTimeOffset.Now;
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Verified;
                _eventBus.Publish(new WithdrawalInvestigated(offlineWithdrawal));
                _repository.SaveChanges();
                scope.Complete();
                return offlineWithdrawal;
            }
        }

        [Permission(Permissions.Fail, Module = Modules.OfflineWithdrawalInvestigation)]
        public OfflineWithdraw FailInvestigation(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                withdrawal.InvestigatedBy = _securityProvider.User.UserName;
                withdrawal.InvestigatedDate = DateTimeOffset.Now;
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Unverified;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();
                scope.Complete();
                return withdrawal;
            }
        }

        [Permission(Permissions.Accept, Module = Modules.OfflineWithdrawalAcceptance)]
        public OfflineWithdraw Accept(OfflineWithdrawId id, string remarks)
        {
            OfflineWithdraw offlineWithdrawal;
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                offlineWithdrawal = GetWithdrawalWithPlayer(id);
                if (offlineWithdrawal.Status != WithdrawalStatus.Verified)
                {
                    ThrowStatusError(offlineWithdrawal.Status, WithdrawalStatus.Accepted);
                }
                offlineWithdrawal.Remarks = remarks;
                offlineWithdrawal.Status = WithdrawalStatus.Accepted;
                offlineWithdrawal.AcceptedBy = _securityProvider.User.UserName;
                offlineWithdrawal.AcceptedTime = DateTimeOffset.Now;
                _eventBus.Publish(new WithdrawalAccepted(offlineWithdrawal));
                _repository.SaveChanges();

                scope.Complete();
            }
            return offlineWithdrawal;
        }

        [Permission(Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance)]
        public OfflineWithdraw Revert(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                if (withdrawal.Status != WithdrawalStatus.Verified)
                {
                    ThrowStatusError(withdrawal.Status, WithdrawalStatus.Reverted);
                }
                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Reverted;
                withdrawal.RevertedBy = _securityProvider.User.UserName;
                withdrawal.RevertedTime = DateTimeOffset.Now;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();
                scope.Complete();
                return withdrawal;
            }
        }

        [Permission(Permissions.Revert, Module = Modules.OfflineWithdrawalAcceptance)]
        public OfflineWithdraw Cancel(OfflineWithdrawId id, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var withdrawal = GetWithdrawalWithPlayer(id);
                if (withdrawal == null)
                    return null;

                withdrawal.Remarks = remarks;
                withdrawal.Status = WithdrawalStatus.Canceled;
                withdrawal.CanceledBy = _securityProvider.User.UserName;
                withdrawal.CanceledTime = DateTimeOffset.Now;
                _eventBus.Publish(new WithdrawalCancelled(_securityProvider.User.UserName)
                {
                    PlayerId = withdrawal.PlayerBankAccount.Player.Id,
                    Status = withdrawal.Status,
                    Remarks = withdrawal.Remarks,
                    Amount = withdrawal.Amount
                });
                _repository.SaveChanges();
                scope.Complete();
                return withdrawal;
            }
        }

        [Permission(Permissions.Exempt, Module = Modules.OfflineWithdrawalExemption)]
        public void SaveExemption(Exemption exemption)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = _paymentQueries.GetPlayer(exemption.PlayerId);
                var brand = _brandQueries.GetBrandOrNull(player.BrandId);
                var exemptFrom = DateTime.Parse(exemption.ExemptFrom);
                var exemptTo = DateTime.Parse(exemption.ExemptTo);

                if (exemptTo < exemptFrom)
                    throw new ArgumentException("ExemptTo must be qreater or equal then ExemptFrom.");

                player.ExemptWithdrawalVerification = exemption.Exempt;
                player.ExemptWithdrawalFrom = exemptFrom.ToBrandDateTimeOffset(brand.TimezoneId);
                player.ExemptWithdrawalTo = exemptTo.ToBrandDateTimeOffset(brand.TimezoneId);
                player.ExemptLimit = exemption.ExemptLimit;
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerAccountRestrictionsChanged(player.ExemptLimit,
                    player.ExemptWithdrawalTo,
                    player.ExemptWithdrawalFrom,
                    player.ExemptWithdrawalVerification));

                scope.Complete();
            }
        }

        static string GenerateTransactionNumber()
        {
            var random = new Random();
            return "OW" + random.Next(10000000, 99999999);
        }
    }
}
