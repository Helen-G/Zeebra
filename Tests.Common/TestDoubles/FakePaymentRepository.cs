using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakePaymentRepository : PaymentRepository, IPaymentRepository
    {
        private readonly FakeDbSet<PaymentLevel> _paymentLevels = new FakeDbSet<PaymentLevel>();
        private readonly FakeDbSet<PaymentSettings> _paymentSettings = new FakeDbSet<PaymentSettings>();
        private readonly FakeDbSet<TransferSettings> _transferSettings = new FakeDbSet<TransferSettings>();
        private readonly FakeDbSet<TransferFund> _transferFund = new FakeDbSet<TransferFund>();
        private readonly FakeDbSet<OfflineDeposit> _offlineDeposits = new FakeDbSet<OfflineDeposit>();
        private readonly FakeDbSet<OfflineWithdraw> _offlineWithdraw = new FakeDbSet<OfflineWithdraw>();
        private readonly FakeDbSet<Bank> _banks = new FakeDbSet<Bank>();
        private readonly FakeDbSet<Brand> _brands = new FakeDbSet<Brand>();
        private readonly FakeDbSet<Licensee> _licensees = new FakeDbSet<Licensee>();
        private readonly FakeDbSet<BrandCurrency> _brandCurrencies = new FakeDbSet<BrandCurrency>();
        private readonly FakeDbSet<CurrencyExchange> _currencyExchanges = new FakeDbSet<CurrencyExchange>();
        private readonly FakeDbSet<VipLevel> _vipLevels = new FakeDbSet<VipLevel>();
        private readonly FakeDbSet<BankAccount> _bankAccount = new FakeDbSet<BankAccount>();
        private readonly FakeDbSet<Player> _players = new FakeDbSet<Player>();
        private readonly FakeDbSet<Currency> _currencies = new FakeDbSet<Currency>();
        private readonly FakeDbSet<PlayerPaymentLevel> _playerPaymentLevels = new FakeDbSet<PlayerPaymentLevel>();
        private readonly FakeDbSet<PlayerBankAccount> _playerBankAccounts = new FakeDbSet<PlayerBankAccount>();

        public override IDbSet<PaymentLevel> PaymentLevels { get { return _paymentLevels; } }
        public override IDbSet<PaymentSettings> PaymentSettings { get { return _paymentSettings; } }
        public override IDbSet<TransferSettings> TransferSettings { get { return _transferSettings; } }
        public override IDbSet<TransferFund> TransferFunds { get { return _transferFund; } }
        public override IDbSet<OfflineDeposit> OfflineDeposits { get { return _offlineDeposits; } }
        public override IDbSet<OfflineWithdraw> OfflineWithdraws { get { return _offlineWithdraw; } }
        public override IDbSet<Bank> Banks { get { return _banks; } }
        public override IDbSet<Brand> Brands { get { return _brands; } }
        public override IDbSet<Licensee> Licensees { get { return _licensees; } }
        public override IDbSet<BrandCurrency> BrandCurrencies { get { return _brandCurrencies; } }
        public override IDbSet<CurrencyExchange> CurrencyExchanges { get { return _currencyExchanges; } }
        public override IDbSet<VipLevel> VipLevels { get { return _vipLevels; } }
        public override IDbSet<BankAccount> BankAccounts { get { return _bankAccount; } }
        public override IDbSet<Player> Players { get { return _players; } }
        public override IDbSet<Currency> Currencies { get { return _currencies; } }
        public override IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get { return _playerPaymentLevels; } }
        public override IDbSet<PlayerBankAccount> PlayerBankAccounts { get { return _playerBankAccounts; } }

        public new OfflineDeposit GetDepositById(Guid id)
        {
            throw new NotImplementedException();
        }

        public new BankAccount GetBankAccount(Guid id)
        {
            throw new NotImplementedException();
        }

        public new int SaveChanges()
        {
            if (!_offlineDeposits.AllElementsAreUnique())
            {
                throw new RegoException("OfflineDeposits with duplicate Ids were found");
            }

            return 0;
        }
    }
}