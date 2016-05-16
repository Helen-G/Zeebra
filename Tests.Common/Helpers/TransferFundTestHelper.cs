using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class TransferFundTestHelper
    {
        private readonly TransferSettingsCommands _transferSettingsCommands;

        public TransferFundTestHelper(TransferSettingsCommands transferSettingsCommands)
        {
            _transferSettingsCommands = transferSettingsCommands;
        }

        public void CreateTransferSettings(Core.Brand.Data.Brand brand, SaveTransferSettingsCommand saveTransferSettingsCommand, TransferFundType transferType = TransferFundType.FundIn)
        {
            var transferSettings = new SaveTransferSettingsCommand
            {
                Brand = brand.Id,
                Licensee = brand.Licensee.Id,
                TimezoneId = brand.TimezoneId,
                TransferType = TransferFundType.FundIn,
                Currency = brand.DefaultCurrency,
                //VipLevel = brand.VipLevels.Single().Code,
                VipLevel = brand.VipLevels.Single().Id,
                Wallet = brand.WalletTemplates.Single(x => !x.IsMain).Id.ToString(),
                MinAmountPerTransaction = saveTransferSettingsCommand.MinAmountPerTransaction,
                MaxAmountPerTransaction = saveTransferSettingsCommand.MaxAmountPerTransaction,
                MaxAmountPerDay = saveTransferSettingsCommand.MaxAmountPerDay,
                MaxTransactionPerDay = saveTransferSettingsCommand.MaxTransactionPerDay,
                MaxTransactionPerWeek = saveTransferSettingsCommand.MaxTransactionPerWeek,
                MaxTransactionPerMonth = saveTransferSettingsCommand.MaxTransactionPerMonth
            };

            var transferSettingsId = _transferSettingsCommands.AddSettings(transferSettings);
            _transferSettingsCommands.Enable(transferSettingsId, brand.TimezoneId, "remark");
        }

        public void CreateTransferSettings(Core.Brand.Data.Brand brand,
            SaveTransferSettingsCommand saveTransferSettingsCommand, TransferFundType transferType, string currencyCode, Guid vipLevel)
        {
            var transferSettings = new SaveTransferSettingsCommand
            {
                Brand = brand.Id,
                Licensee = brand.Licensee.Id,
                TimezoneId = brand.TimezoneId,
                TransferType = transferType,
                Currency = currencyCode,
                //VipLevel = vipLevel.ToString(),
                VipLevel = vipLevel,
                Wallet = brand.WalletTemplates.Single(x => !x.IsMain).Id.ToString(),
                MinAmountPerTransaction = saveTransferSettingsCommand.MinAmountPerTransaction,
                MaxAmountPerTransaction = saveTransferSettingsCommand.MaxAmountPerTransaction,
                MaxAmountPerDay = saveTransferSettingsCommand.MaxAmountPerDay,
                MaxTransactionPerDay = saveTransferSettingsCommand.MaxTransactionPerDay,
                MaxTransactionPerWeek = saveTransferSettingsCommand.MaxTransactionPerWeek,
                MaxTransactionPerMonth = saveTransferSettingsCommand.MaxTransactionPerMonth
            };

            var transferSettingsId = _transferSettingsCommands.AddSettings(transferSettings);
            _transferSettingsCommands.Enable(transferSettingsId, brand.TimezoneId, "remark");
        }

        public void AddTransfer(Core.Brand.Data.Brand brand, Guid playerId, IPaymentRepository repository, double timeShiftInDays = 0)
        {
            var transferFund = new TransferFund
            {
                Id = Guid.NewGuid(),
                Amount = 1,
                CreatedBy = playerId.ToString(),
                Created = DateTimeOffset.Now.ToBrandOffset(brand.TimezoneId) + TimeSpan.FromDays(timeShiftInDays),
                TransferType = TransferFundType.FundIn,
                WalletId = brand.WalletTemplates.Single(x => !x.IsMain).Id.ToString(),
                Status = TransferFundStatus.Approved,
                TransactionNumber = "TF000000000"
            };

            repository.TransferFunds.Add(transferFund);
            repository.SaveChanges();
        }
    }
}
