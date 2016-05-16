using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public interface IPaymentQueries : IBasePaymentQueries
    {
        IEnumerable<OfflineDeposit> GetDeposits();
        OfflineDeposit GetDepositById(Guid id);
        OfflineWithdraw GetWithdrawById(Guid id);
        Bank GetBank(Guid id);
        List<Bank> GetBanksByBrand(Guid brandId);
        BankAccount GetBankAccount(Guid id);
        PaymentSettings GetPaymentSettings(Guid id);
        IQueryable<PaymentSettings> GetPaymentSettings();
        TransferSettings GetTransferSettings(Guid id);
        IQueryable<TransferSettings> GetTransferSettings();
        Data.Player GetPlayer(Guid playerId);
        Data.Player GetPlayerForNewBankAccount(PlayerId playerId);
        Data.Player GetPlayerWithBank(Guid playerId);
        PaymentSettings GetGatewaySettings(Guid brandId, PaymentType type, string vipLevel, Guid bankAccountId);
        PlayerBankAccount GetPlayerBankAccountForEdit(PlayerBankAccountId id);
        List<BankAccount> GetBankAccounts(Guid brandId, string currencyCode);
        PaymentSettingTransferObj GetPaymentSettingById(Guid id);
        Dictionary<Guid, string> GetBankAccountsForOfflineDepositRequest(Guid playerId);
        List<VipLevel> VipLevels();
        VipLevel GetVipLevel(Guid id);
    }
}