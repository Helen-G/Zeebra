using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.ApplicationServices
{
    public class TransferFundCommands : MarshalByRefObject, ITransferFundCommands
    {
        private readonly IPaymentRepository _repository;
        private readonly ITransferFundValidationService _validationService;
        private readonly BrandQueries _brandQueries;
        private readonly IWalletCommands _walletCommands;
        private readonly IEventBus _eventBus;

        public TransferFundCommands(
            IPaymentRepository repository,
            ITransferFundValidationService validationService,
            BrandQueries brandQueries,
            IWalletCommands walletCommands,
            IEventBus eventBus)
        {
            _repository = repository;
            _validationService = validationService;
            _brandQueries = brandQueries;
            _walletCommands = walletCommands;
            _eventBus = eventBus;
        }

        public string AddFund(TransferFundRequest request)
        {
            var validationResuult = _validationService.Validate(request);

            var transferFund = new TransferFund();
            var transactionNumber = GenerateTransactionNumber();

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var requestWalletId = new Guid(request.WalletId);
                var requestWalletTemplate = _brandQueries.GetWalletTemplate(requestWalletId);
                var mainWalletTemplate = _brandQueries.GetWalletTemplates(requestWalletTemplate.Brand.Id).Single(wt => wt.IsMain);
                var timezoneId = requestWalletTemplate.Brand.TimezoneId;

                _repository.TransferFunds.Add(transferFund);
                transferFund.Id = Guid.NewGuid();
                transferFund.TransferType = request.TransferType;
                transferFund.TransactionNumber = transactionNumber;
                transferFund.Amount = request.Amount;
                transferFund.WalletId = request.WalletId;
                transferFund.CreatedBy = request.PlayerId.ToString();
                transferFund.Created = DateTimeOffset.Now.ToBrandOffset(timezoneId);
                transferFund.Remarks = transferFund.Remarks = !validationResuult.IsValid ? validationResuult.ErrorMessage : string.Empty;
                transferFund.Status = validationResuult.IsValid ? TransferFundStatus.Approved : TransferFundStatus.Rejected;
                transferFund.BonusCode = request.BonusCode;

                if (validationResuult.IsValid)
                {
                    var sourceWalletId = request.TransferType == TransferFundType.FundIn
                        ? mainWalletTemplate.Id
                        : requestWalletId;
                    var destinationWalletId = request.TransferType == TransferFundType.FundIn
                        ? requestWalletId
                        : mainWalletTemplate.Id;

                    _walletCommands.TransferFunds(request.PlayerId, sourceWalletId, destinationWalletId, request.Amount, transactionNumber);

                    transferFund.DestinationWalletId = destinationWalletId;
                }

                _eventBus.Publish(new TransferFundCreated
                {
                    Created = transferFund.Created,
                    PlayerId = new Guid(transferFund.CreatedBy),
                    TransactionNumber = transferFund.TransactionNumber,
                    Amount = transferFund.Amount,
                    Remarks = transferFund.Remarks,
                    BonusCode = transferFund.BonusCode,
                    DestinationWalletStructureId = transferFund.DestinationWalletId,
                    Type = transferFund.TransferType,
                    Status = transferFund.Status,
                    Description = string.Format("Transaction #{0}", transferFund.TransactionNumber)
                });
                _repository.SaveChanges();
                scope.Complete();
            }

            if (!validationResuult.IsValid)
                throw new ArgumentException(validationResuult.ErrorMessage);

            return transactionNumber;
        }

        //todo: Add Unique key to DB
        private static string GenerateTransactionNumber()
        {
            //alternative
            //byte[] guildBytes = Guid.NewGuid().ToByteArray();
            //var id = BitConverter.ToInt64(guildBytes, 0);
            var random = new Random();
            return "TF" + random.Next(10000000, 99999999);
        }
    }
}
