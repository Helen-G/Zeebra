using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Domain.Payment
{
    public interface IPaymentRepository
    {
        IDbSet<PaymentLevel> PaymentLevels { get; }
        IDbSet<PaymentSettings> PaymentSettings { get; }
        IDbSet<OfflineDeposit> OfflineDeposits { get; }
        IDbSet<OfflineWithdraw> OfflineWithdraws { get; }
        IDbSet<Bank> Banks { get; }
        IDbSet<BankAccount> BankAccounts { get; }
        IDbSet<PlayerBankAccount> PlayerBankAccounts { get; }
        IDbSet<Data.Brand> Brands { get; }
        IDbSet<Data.Licensee> Licensees { get; }
        IDbSet<Data.VipLevel> VipLevels { get; }
        IDbSet<Core.Payment.Data.Player> Players { get; }
        IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get; }
        IDbSet<TransferSettings> TransferSettings { get; }
        IDbSet<TransferFund> TransferFunds { get; }
        int SaveChanges();
        Entities.OfflineDeposit GetDepositById(Guid id);
        Entities.BankAccount GetBankAccount(Guid id);
        Entities.OfflineDeposit CreateOfflineDeposit(Guid id, string number, OfflineDepositRequest request, Guid bankAccountId, Core.Payment.Data.Player player);

        IDbSet<Currency> Currencies { get; }
        IDbSet<CurrencyExchange> CurrencyExchanges { get; }
        IDbSet<BrandCurrency> BrandCurrencies { get; }
    }
}