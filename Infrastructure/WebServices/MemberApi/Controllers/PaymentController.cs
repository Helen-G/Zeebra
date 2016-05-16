using System;
using System.Linq;
using System.Web.Http;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Infrastructure;
using AFT.RegoV2.MemberApi.Interface.Common;
using AFT.RegoV2.MemberApi.Interface.Payment;
using AutoMapper;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly BrandQueries _brandQueries;
        private readonly WithdrawalService _withdrawalService;
        private readonly WalletCommands _walletCommands;
        private readonly ITransferFundCommands _transferFundCommands;
        private readonly OfflineDepositCommands _offlineDepositCommands;
        private readonly OfflineDepositQueries _offlineDepositQueries;
        private readonly IFileStorage _fileStarage;

        public PaymentController(
            IPaymentQueries paymentQueries,
            BrandQueries brandQueries,
            WithdrawalService withdrawalService,
            ITransferFundCommands transferFundCommands,
            OfflineDepositCommands offlineDepositCommands,
            OfflineDepositQueries offlineDepositQueries,
            WalletCommands walletCommands,
            IFileStorage fileStarage)
        {
            _paymentQueries = paymentQueries;
            _brandQueries = brandQueries;
            _withdrawalService = withdrawalService;
            _walletCommands = walletCommands;
            _fileStarage = fileStarage;
            _transferFundCommands = transferFundCommands;
            _offlineDepositCommands = offlineDepositCommands;
            _offlineDepositQueries = offlineDepositQueries;
        }

        [HttpPost]
        public QuickDepositResponse QuickDeposit(QuickDepositRequest request)
        {
            _walletCommands.Deposit(PlayerId, request.Amount, "");
            return new QuickDepositResponse();
        }

        [HttpPost]
        public OfflineDepositFormDataResponse OfflineDepositFormData(OfflineDepositFormDataRequest request)
        {
            return new OfflineDepositFormDataResponse
            {
                BankAccounts = _paymentQueries.GetBankAccountsForOfflineDepositRequest(PlayerId)
            };

        }

        [HttpPost]
        public PendingDepositsResponse PendingDeposits(PendingDepositsRequest request)
        {
            return new PendingDepositsResponse
            {
                PendingDeposits = _offlineDepositQueries.GetPendingDeposits(PlayerId)
                    .Select(o => new OfflineDeposit
                    {
                        Id = o.Id,
                        Amount = o.Amount,
                        Status = o.Status.ToString(),
                        DateCreated = o.Created.GetNormalizedDateTime(),
                        ReferenceCode = o.TransactionNumber
                    })
            };
        }

        [HttpPost]
        public OfflineDeposit GetOfflineDeposit(GetOfflineDepositRequest request)
        {
            var deposit = _offlineDepositQueries.GetOfflineDeposit(request.Id);
            return new OfflineDeposit
            {
                Id = deposit.Id,
                Amount = deposit.Amount,
                Status = deposit.Status.ToString(),
                ReferenceCode = deposit.TransactionNumber,
                DateCreated = deposit.Created.ToString(),
                DepositType = deposit.DepositMethod.ToString(),
                TransferType = deposit.TransferType.ToString()/*,
                IdFront = !string.IsNullOrEmpty(deposit.IdFrontImage) ? _fileStarage.Get(deposit.IdFrontImage) : new byte[] { },
                IdBack = !string.IsNullOrEmpty(deposit.IdBackImage) ? _fileStarage.Get(deposit.IdBackImage) : new byte[] { },
                Receipt = !string.IsNullOrEmpty(deposit.ReceiptImage) ? _fileStarage.Get(deposit.ReceiptImage) : new byte[] { }*/
            };
        }

        [HttpPost]
        public string ConfirmDeposit(OfflineDepositConfirmRequest request)
        {
            var offlineDeposit = _offlineDepositCommands.Confirm(
                     request.DepositConfirm,
                     request.IdFrontImage,
                     request.IdBackImage,
                     request.ReceiptImage);

            return string.Empty;
        }

        [Authorize]
        public WithdrawalFormDataResponse WithdrawalFormData(WithdrawalFormDataRequest request)
        {
            var player = _paymentQueries.GetPlayerWithBank(PlayerId);
            var playerBankAccount = player.CurrentBankAccount;

            if (playerBankAccount == null)
                return new WithdrawalFormDataResponse { BankAccount = new BankData() };

            return new WithdrawalFormDataResponse
            {
                BankAccount = new BankData
                {
                    BankAccountName = playerBankAccount.AccountName,
                    BankAccountNumber = playerBankAccount.AccountNumber,
                    BankName = playerBankAccount.Bank.Name,
                    City = playerBankAccount.City,
                    Branch = playerBankAccount.Branch,
                    Province = playerBankAccount.Province,
                    SwiftCode = playerBankAccount.SwiftCode,
                    PlayerBankAccountId = playerBankAccount.Id,
                    BankAccountTime = playerBankAccount.Updated ?? playerBankAccount.Created,
                    BankTime = playerBankAccount.Bank.Updated ?? playerBankAccount.Bank.Created
                }
            };
        }

        [HttpPost]
        public OfflineDepositResponse OfflineDeposit(OfflineDepositRequest request)
        {
            var offlineDepositRequest = Mapper.DynamicMap<Domain.Payment.Commands.OfflineDepositRequest>(request);
            offlineDepositRequest.PlayerId = PlayerId;
            offlineDepositRequest.RequestedBy = Username;

            _offlineDepositCommands.Submit(offlineDepositRequest);


            return new OfflineDepositResponse();
        }

        [HttpPost]
        public OfflineWithdrawalResponse OfflineWithdraw(OfflineWithdrawalRequest request)
        {
            NotificationType notificationMethod;
            Enum.TryParse(request.NotificationType, out notificationMethod);

            _withdrawalService.ProcessWithdrawal(new OfflineWithdrawRequest
            {
                Amount = request.Amount,
                PlayerBankAccountId = request.PlayerBankAccountId,
                RequestedBy = Username,
                NotificationType = notificationMethod,
                BankTime = request.BankTime,
                BankAccountTime = request.BankAccountTime
            });

            return new OfflineWithdrawalResponse();
        }

        [HttpPost]
        public FundTransferFormDataResponse FundTransferFormData(FundTransferFormDataRequest request)
        {
            return new FundTransferFormDataResponse
            {
                Wallets = _brandQueries.GetWalletTemplates(request.BrandId).Where(wallet => !wallet.IsMain).ToDictionary(w => w.Id, w => w.Name)
            };
        }

        [HttpPost]
        public FundResponse FundIn(FundRequest request)
        {
            var transferFundRequest = new TransferFundRequest
            {
                PlayerId = PlayerId,
                Amount = request.Amount,
                TransferType = request.TransferFundType,
                WalletId = request.WalletId.ToString(),
                BonusCode = request.BonusCode
            };

            return new FundResponse
            {
                TransferId = _transferFundCommands.AddFund(transferFundRequest)
            };
        }
    }
}