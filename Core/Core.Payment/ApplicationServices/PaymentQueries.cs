using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Shared.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;
using Brand = AFT.RegoV2.Domain.Payment.Data.Brand;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class PaymentQueries : MarshalByRefObject, IPaymentQueries
    {
        private readonly IPaymentRepository _repository;

        private readonly IPlayerQueries _playerQueries;

        private readonly BrandQueries _brandQueries;

        public PaymentQueries(
            IPaymentRepository repository,
            IPlayerQueries playerQueries,
            BrandQueries brandQueries)
        {
            _repository = repository;
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;
        }

        public IEnumerable<OfflineDeposit> GetDeposits()
        {
            var deposits = _repository.OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank);

            return deposits;
        }

        public OfflineDeposit GetDepositById(Guid id)
        {
            var offlineDeposit = _repository.OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank)
                .FirstOrDefault(x => x.Id == id);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit with Id {0} was not found", id));
            }
            return offlineDeposit;
        }

        public OfflineDeposit GetDepositByPlayerId(Guid playerId)
        {
            var offlineDeposit = _repository.OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank)
                .FirstOrDefault(x => x.Player.Id == playerId);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit for player's Id {0} was not found", playerId));
            }
            return offlineDeposit;
        }

        public OfflineDeposit GetLastDepositByPlayerId(Guid playerId)
        {
            var offlineDeposit = _repository.OfflineDeposits
                .Include(x => x.Brand)
                .Include(x => x.Player)
                .Include(x => x.BankAccount.Bank)
                .OrderByDescending(x => x.Created)
                .FirstOrDefault(x => x.Player.Id == playerId);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit for player's Id {0} was not found", playerId));
            }
            return offlineDeposit;
        }

        public IEnumerable<BankAccount> GetBankAccountsForAdminOfflineDepositRequest(Guid playerId)
        {
            return GetBankAccountsForOfflineDeposit(playerId);
        }

        public Dictionary<Guid, string> GetBankAccountsForOfflineDepositRequest(Guid playerId)
        {
            var bankAccounts = GetBankAccountsForOfflineDeposit(playerId);
            if (bankAccounts == null || !bankAccounts.Any())
                return new Dictionary<Guid, string>();

            return bankAccounts.ToDictionary(
                    account => account.Id,
                    account => String.Format("{0} / {1}", account.Bank.Name, account.AccountName));
        }

        private IEnumerable<BankAccount> GetBankAccountsForOfflineDeposit(Guid playerId)
        {
            var player = _playerQueries.GetPlayer(playerId);
            if (player == null)
            {
                throw new ArgumentException(@"Player was not found", "playerId");
            }

            var paymentLevel = _repository.PlayerPaymentLevels
                .Include(x => x.PaymentLevel.BankAccounts.Select(s => s.Bank))
                .Single(l => l.PlayerId == playerId)
                .PaymentLevel;

            if (!paymentLevel.EnableOfflineDeposit)
            {
                return new BankAccount[] { };
            }

            var bankAccounts = paymentLevel.BankAccounts.Where(x => x.Status == BankAccountStatus.Active);
            return bankAccounts;
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositRequests)]
        public OfflineDeposit GetDepositByIdForViewRequest(OfflineDepositId id)
        {
            return GetDepositById(id);
        }

        [Permission(Permissions.Confirm, Module = Modules.OfflineDepositConfirmation)]
        public OfflineDeposit GetDepositByIdForConfirmation(OfflineDepositId id)
        {
            return GetDepositById(id);
        }

        public OfflineWithdraw GetWithdrawById(Guid id)
        {
            var offlineWithdraw = _repository.OfflineWithdraws
                .Include(x => x.PlayerBankAccount.Player)
                .Include(x => x.PlayerBankAccount.Bank.Brand)
                .FirstOrDefault(x => x.Id == id);
            if (offlineWithdraw == null)
            {
                throw new ArgumentException(string.Format("OfflineWithdraw with Id {0} was not found", id));
            }
            return offlineWithdraw;
        }

        public PaymentSettings GetGatewaySettings(Guid brandId, PaymentType type, string vipLevel, Guid bankAccountId)
        {
            return _repository.PaymentSettings.SingleOrDefault(x =>
                x.BrandId == brandId &&
                x.PaymentType == type && x.VipLevel == vipLevel && x.PaymentGateway.BankAccount.Id == bankAccountId && x.Enabled);
        }

        public Bank GetBank(Guid id)
        {
            var bank = _repository.Banks
              .First(x => x.Id == id);

            if (bank == null)
            {
                throw new ArgumentException("Bank not found");
            }
            return bank;
        }

        public List<Bank> GetBanksByBrand(Guid brandId)
        {
            return _repository.Banks.Where(x => x.BrandId == brandId).ToList();
        }

        public List<BankAccount> GetBankAccounts(Guid brandId, string currencyCode)
        {
            return _repository
                .BankAccounts
                .Where(x => x.Bank.BrandId == brandId && x.CurrencyCode == currencyCode)
                .ToList();
        }

        public PaymentSettingTransferObj GetPaymentSettingById(Guid id)
        {
            var paymentSettings = GetPaymentSettings(id);

            var obj = new PaymentSettingTransferObj
            {
                Brand = new
                {
                    paymentSettings.Brand.Id,
                    paymentSettings.Brand.Name,
                    Licensee = new
                    {
                        id = paymentSettings.Brand.LicenseeId
                    }
                },
                PaymentType = paymentSettings.PaymentType.ToString(),
                CurrencyCode = paymentSettings.CurrencyCode,
                VipLevel = _playerQueries.VipLevels.Single(x => x.Id == new Guid(paymentSettings.VipLevel)).Name,
                Id = paymentSettings.PaymentGateway.Id,
                PaymentMethod = "Offline - " + paymentSettings.PaymentGateway.BankAccount.AccountId,
                MinAmountPerTransaction = paymentSettings.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSettings.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSettings.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSettings.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSettings.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSettings.MaxTransactionPerMonth,
            };

            return obj;
        }

        public IQueryable<BankAccount> GetBankAccounts()
        {
            return _repository.BankAccounts;
        }

        [Permission(Permissions.View, Module = Modules.Banks)]
        public IQueryable<Bank> GetBanks()
        {
            return _repository.Banks;
        }

        public BankAccount GetBankAccount(Guid id)
        {
            var bankAccount = _repository.BankAccounts
                .Include(x => x.Bank)
                .FirstOrDefault(x => x.Id == id);

            if (bankAccount == null)
            {
                throw new ArgumentException("Bank account not found");
            }
            return bankAccount;
        }

        public PaymentSettings GetPaymentSettings(Guid id)
        {
            var paymentSettings = _repository.PaymentSettings
                .Include(x => x.Brand)
                .Include(x => x.PaymentGateway)
                .Include(x => x.PaymentGateway.BankAccount)
                .SingleOrDefault(x => x.Id == id);

            if (paymentSettings == null)
            {
                throw new ArgumentException("Payment settings not found");
            }
            return paymentSettings;
        }

        [Permission(Permissions.View, Module = Modules.PaymentSettings)]
        public IQueryable<PaymentSettings> GetPaymentSettings()
        {
            return _repository.PaymentSettings
                .Include(x => x.Brand)
                .Include(x => x.PaymentGateway.BankAccount)
                .AsQueryable();
        }

        public IEnumerable<PaymentLevelDTO> GetPaymentLevels()
        {
            var levels = from x in _repository.PaymentLevels.ToList()
                         let isDefault = _brandQueries.GetDefaultPaymentLevelId(x.BrandId, x.CurrencyCode) == x.Id
                         select new PaymentLevelDTO
                         {
                             Id = x.Id,
                             Name = x.Name,
                             Code = x.Code,
                             BrandId = x.BrandId,
                             CurrencyCode = x.CurrencyCode,
                             IsDefault = isDefault
                         };

            return levels;
        }

        [Permission(Permissions.View, Module = Modules.PaymentLevelManager)]
        public IQueryable<PaymentLevel> GetPaymentLevelsAsQueryable()
        {
            return _repository.PaymentLevels;
        }

        public PaymentSettingDTO GetPaymentSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel, PaymentType type)
        {
            if (vipLevel == null)
                return null;

            var paymentSetting = _repository
                .PaymentSettings
                .FirstOrDefault(
                    ps => ps.BrandId == brandId &&
                    ps.CurrencyCode == currencyCode &&
                    ps.VipLevel == vipLevel.Id.ToString()
                    && ps.PaymentType == type);

            if (paymentSetting == null)
                return null;

            return new PaymentSettingDTO
            {
                CurrencyCode = paymentSetting.CurrencyCode,
                VipLevel = paymentSetting.VipLevel,
                MinAmountPerTransaction = paymentSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSetting.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSetting.MaxTransactionPerMonth
            };
        }

        public Brand GetBrand(Guid id)
        {
            return _repository.Brands
                .FirstOrDefault(x => x.Id == id);
        }

        public VipLevel GetVipLevel(Guid id)
        {
            return _repository.VipLevels
                .FirstOrDefault(x => x.Id == id);
        }

        public List<VipLevel> VipLevels()
        {
            return _repository.VipLevels.ToList();
        }

        public TransferSettings GetTransferSettings(Guid id)
        {
            var transferSettings = _repository.TransferSettings
                .Include(x => x.Brand)
                .Include(x => x.VipLevel)
                .SingleOrDefault(x => x.Id == id);

            if (transferSettings == null)
            {
                throw new ArgumentException("Transfer settings not found");
            }
            return transferSettings;
        }

        [Permission(Permissions.View, Module = Modules.TransferSettings)]
        public IQueryable<TransferSettings> GetTransferSettings()
        {
            return _repository.TransferSettings
                .Include(x => x.Brand)
                .Include(x => x.VipLevel)
                .AsQueryable();
        }

        public TransferSettingDTO GetTransferSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel)
        {
            if (vipLevel == null)
                return null;

            var paymentSetting = _repository
                .TransferSettings
                .FirstOrDefault(ps => ps.BrandId == brandId && ps.CurrencyCode == currencyCode && ps.VipLevelId == vipLevel.Id);

            if (paymentSetting == null)
                return null;

            return new TransferSettingDTO
            {
                CurrencyCode = paymentSetting.CurrencyCode,
                VipLevel = paymentSetting.VipLevelId.ToString(),
                MinAmountPerTransaction = paymentSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = paymentSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = paymentSetting.MaxAmountPerDay,
                MaxTransactionPerDay = paymentSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = paymentSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = paymentSetting.MaxTransactionPerMonth
            };
        }

        public TransferSettingDTO GetTransferSetting(string walletId, TransferFundType transferFundType, bool enabled)
        {
            var transferSetting = _repository
                .TransferSettings
                .FirstOrDefault(
                        ts => ts.WalletId.Equals(walletId) &&
                        ts.TransferType == transferFundType &&
                        ts.Enabled == enabled);

            if (transferSetting == null)
                return null;

            return new TransferSettingDTO
            {
                CurrencyCode = transferSetting.CurrencyCode,
                VipLevel = transferSetting.VipLevelId.ToString(),
                MinAmountPerTransaction = transferSetting.MinAmountPerTransaction,
                MaxAmountPerTransaction = transferSetting.MaxAmountPerTransaction,
                MaxAmountPerDay = transferSetting.MaxAmountPerDay,
                MaxTransactionPerDay = transferSetting.MaxTransactionPerDay,
                MaxTransactionPerWeek = transferSetting.MaxTransactionPerWeek,
                MaxTransactionPerMonth = transferSetting.MaxTransactionPerMonth
            };
        }

        [Permission(Permissions.Add, Module = Modules.PlayerBankAccount)]
        public Core.Payment.Data.Player GetPlayerForNewBankAccount(PlayerId playerId)
        {
            return GetPlayerHelper(playerId, q => q.Include(x => x.CurrentBankAccount));
        }

        public Core.Payment.Data.Player GetPlayerWithBank(Guid playerId)
        {
            return GetPlayerHelper(playerId, q => q.Include(x => x.CurrentBankAccount.Bank));
        }

        public Core.Payment.Data.Player GetPlayer(Guid playerId)
        {
            return GetPlayerHelper(playerId, null);
        }

        public Core.Payment.Data.Player GetPlayerHelper(Guid playerId, Func<IQueryable<Core.Payment.Data.Player>, IQueryable<Core.Payment.Data.Player>> modifyQuery)
        {
            var queryable = _repository.Players.AsQueryable();
            if (modifyQuery != null)
            {
                queryable = modifyQuery(queryable);
            }
            var playerData = queryable.SingleOrDefault(x => x.Id == playerId);
            if (playerData != null)
            {
                return playerData;
            }

            var player = _playerQueries.GetPlayer(playerId);
            if (player == null)
            {
                throw new ArgumentException("Player not found");
            }
            playerData = new Core.Payment.Data.Player();
            playerData.Id = player.Id;
            playerData.DomainName = player.DomainName;
            playerData.Username = player.Username;
            playerData.FirstName = player.FirstName;
            playerData.LastName = player.LastName;
            playerData.Address = player.MailingAddressLine1;
            playerData.ZipCode = player.MailingAddressPostalCode;
            playerData.Email = player.Email;
            playerData.PhoneNumber = player.PhoneNumber;
            playerData.CurrencyCode = player.CurrencyCode;
            playerData.BrandId = player.BrandId;
            playerData.HousePlayer = player.InternalAccount;
            playerData.DateRegistered = player.DateRegistered;
            playerData.VipLevelId = player.VipLevel.Id;
            _repository.Players.Add(playerData);
            _repository.SaveChanges();
            return playerData;
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositRequests)]
        public IDbSet<OfflineDeposit> GetOfflineDeposits()
        {
            return _repository.OfflineDeposits;
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositConfirmation)]
        public IDbSet<OfflineDeposit> GetDepositsAsConfirmed()
        {
            return _repository.OfflineDeposits;
        }

        [Permission(Permissions.View, Module = Modules.OfflineDepositVerification)]
        public IDbSet<OfflineDeposit> GetDepositsAsVerified()
        {
            return _repository.OfflineDeposits;
        }

        [Permission(Permissions.View, Module = Modules.PlayerBankAccount)]
        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts()
        {
            return _repository.PlayerBankAccounts
                .Include(x => x.Player.CurrentBankAccount)
                .Include(x => x.Bank);
        }

        [Permission(Permissions.Edit, Module = Modules.PlayerBankAccount)]
        public PlayerBankAccount GetPlayerBankAccountForEdit(PlayerBankAccountId id)
        {
            return _repository.PlayerBankAccounts
                .Include(x => x.Player.CurrentBankAccount)
                .Include(x => x.Bank)
                .SingleOrDefault(x => x.Id == id);
        }

        [Permission(Permissions.Edit, Module = Modules.PlayerBankAccount)]
        public PlayerBankAccount GetPlayerBankAccountForSetCurrent(PlayerBankAccountId id)
        {
            return _repository.PlayerBankAccounts.Include(x => x.Player).SingleOrDefault(x => x.Id == id);
        }

        public IQueryable<PlayerPaymentLevel> GetPlayerPaymentLevels()
        {
            return _repository.PlayerPaymentLevels;
        }

        public PaymentLevel GetPaymentLevel(Guid id)
        {
            return _repository.PaymentLevels
                .Include(l => l.Brand)
                .Include(l => l.BankAccounts)
                .SingleOrDefault(l => l.Id == id);
        }

        public IQueryable<OfflineWithdraw> GetOfflineWithdraws()
        {
            return _repository.OfflineWithdraws;
        }

        public IQueryable<TransferFund> GetTransferFunds()
        {
            return _repository.TransferFunds;
        }

        #region currency

        public IQueryable<Currency> GetCurrencies()
        {
            return _repository.Currencies.AsNoTracking();
        }

        public IQueryable<Currency> GetCurrencies(bool isActive)
        {
            //todo: currency: when conditions of not active currency appear
            //return _repository.Currencies.Where(c => c.Status == (isActive ? CurrencyStatus.Active : CurrencyStatus.Inactive)).AsNoTracking();
            return _repository.Currencies.AsNoTracking();
        }

        public Currency GetCurrency(string currencyCode)
        {
            return GetCurrencies().SingleOrDefault(x => x.Code == currencyCode);
        }

        #endregion currency

        #region currency exchange

        public IQueryable<CurrencyExchange> GetCurrencyExchanges()
        {
            return _repository.CurrencyExchanges.AsNoTracking();
        }

        public IQueryable<CurrencyExchange> GetCurrencyExchangesbyBrand(Guid brandId)
        {
            return _repository.CurrencyExchanges
                .Include(x => x.Brand)
                .Include(x => x.CurrencyTo)
                .AsNoTracking();
        }

        public CurrencyExchange GetCurrencyExchange(Guid brandId, string currencyCode)
        {
            var currencyExchange = _repository.CurrencyExchanges
                .Include(x => x.Brand)
                .Include(x => x.CurrencyTo)
                .SingleOrDefault(x => x.Brand.Id == brandId && x.CurrencyTo.Code == currencyCode);

            if (currencyExchange == null)
            {
                throw new RegoException("Currency exchange not found");
            }
            return currencyExchange;
        }

        public IQueryable<Licensee> GetLicensees()
        {
            return _repository.Licensees;
        }

        public IQueryable<Brand> GetBrands()
        {
            return _repository.Brands;
        }

        public IEnumerable<BrandCurrency> GetBrandCurrencies(Guid brandId)
        {
            return _repository.BrandCurrencies.Where(b => b.BrandId == brandId);
        }

        #endregion
    }

    public class PaymentSettingTransferObj
    {
        public object Brand { get; set; }
        public string PaymentMethod { get; set; }
        public string VipLevel { get; set; }
        public string CurrencyCode { get; set; }
        public decimal MinAmountPerTransaction { get; set; }
        public decimal MaxAmountPerTransaction { get; set; }
        public decimal MaxAmountPerDay { get; set; }
        public int MaxTransactionPerDay { get; set; }
        public int MaxTransactionPerWeek { get; set; }
        public int MaxTransactionPerMonth { get; set; }
        public Guid Id { get; set; }
        public string PaymentType { get; set; }
    }
}
