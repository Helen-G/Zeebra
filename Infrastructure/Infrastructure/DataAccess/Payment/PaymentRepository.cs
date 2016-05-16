using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Mappings;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Migrations;

namespace AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository
{
    public class PaymentRepository : DbContext, IPaymentRepository, ISeedable
    {
        private readonly Guid _brand138Id = new Guid("00000000-0000-0000-0000-000000000138");
        private readonly Guid _brand831Id = new Guid("D2CFDF01-FB79-4365-9E47-FC8E4DC1349C");

        static PaymentRepository()
        {
            Database.SetInitializer(new PaymentRepositoryInitializer());
        }

        public PaymentRepository()
            : base("name=Default")
        {
        }

        public virtual IDbSet<PaymentLevel> PaymentLevels { get; set; }
        public virtual IDbSet<PaymentSettings> PaymentSettings { get; set; }
        public virtual IDbSet<OfflineDeposit> OfflineDeposits { get; set; }
        public virtual IDbSet<OfflineWithdraw> OfflineWithdraws { get; set; }
        public virtual IDbSet<Bank> Banks { get; set; }
        public virtual IDbSet<Domain.Payment.Data.Brand> Brands { get; set; }
        public virtual IDbSet<Licensee> Licensees { get; set; }
        public virtual IDbSet<BrandCurrency> BrandCurrencies { get; set; }
        public virtual IDbSet<VipLevel> VipLevels { get; set; }
        public virtual IDbSet<BankAccount> BankAccounts { get; set; }
        public virtual IDbSet<PlayerBankAccount> PlayerBankAccounts { get; set; }
        public virtual IDbSet<Core.Payment.Data.Player> Players { get; set; }
        public virtual IDbSet<PlayerPaymentLevel> PlayerPaymentLevels { get; set; }
        public virtual IDbSet<TransferSettings> TransferSettings { get; set; }
        public virtual IDbSet<TransferFund> TransferFunds { get; set; }
        public virtual IDbSet<Currency> Currencies { get; set; }
        public virtual IDbSet<CurrencyExchange> CurrencyExchanges { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new BrandMap());
            modelBuilder.Configurations.Add(new LicenseeMap());
            modelBuilder.Configurations.Add(new BrandCurrencyMap());
            modelBuilder.Configurations.Add(new VipLevelMap());
            modelBuilder.Configurations.Add(new BankMap());
            modelBuilder.Configurations.Add(new BankAccountMap());
            modelBuilder.Configurations.Add(new PlayerBankAccountMap());
            modelBuilder.Configurations.Add(new OfflineDepositMap());
            modelBuilder.Configurations.Add(new OfflineWithdrawMap());
            modelBuilder.Configurations.Add(new PaymentLevelMap());
            modelBuilder.Configurations.Add(new PaymentSettingsMap());
            modelBuilder.Configurations.Add(new PaymentGatewayMap());
            modelBuilder.Configurations.Add(new PlayerPaymentLevelMap());
            modelBuilder.Configurations.Add(new PlayerMap());
            modelBuilder.Configurations.Add(new TransferSettingsMap());
            modelBuilder.Configurations.Add(new TransferFundMap());
            modelBuilder.Configurations.Add(new CurrencyMap());
            modelBuilder.Configurations.Add(new CurrencyExchangeMap());
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var dbValidationErrorMessages = e.EntityValidationErrors.ToArray();
                Trace.WriteLine(dbValidationErrorMessages.ToString());
                throw;
            }
        }

        public Domain.Payment.Entities.OfflineDeposit CreateOfflineDeposit(
            Guid id,
            string number,
            OfflineDepositRequest request,
            Guid bankAccountId,
            Core.Payment.Data.Player player)
        {
            OfflineDeposit offlineDeposit;
            var bankAccount = BankAccounts.Single(ba => ba.Id == bankAccountId);
            var deposit = new Domain.Payment.Entities.OfflineDeposit(id, number, request, bankAccount, player.BrandId, out offlineDeposit);
            OfflineDeposits.Add(offlineDeposit);
            return deposit;
        }

        public Domain.Payment.Entities.OfflineDeposit GetDepositById(Guid id)
        {
            OfflineDeposit offlineDeposit = OfflineDeposits
                .Include(p => p.BankAccount.Bank)
                .Include(p => p.Player)
                .FirstOrDefault(x => x.Id == id);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit with Id {0} was not found", id));
            }
            return new Domain.Payment.Entities.OfflineDeposit(offlineDeposit);
        }

        public Domain.Payment.Entities.BankAccount GetBankAccount(Guid id)
        {
            var bankAccount = BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.PaymentLevels)
                .First(x => x.Id == id);

            if (bankAccount == null)
            {
                throw new ArgumentException("Bank not found");
            }
            return new Domain.Payment.Entities.BankAccount(bankAccount);
        }
        public OfflineDeposit GetDepositDataById(Guid id)
        {
            OfflineDeposit offlineDeposit = OfflineDeposits
                .Include(p => p.BankAccount.Bank).FirstOrDefault(x => x.Id == id);
            if (offlineDeposit == null)
            {
                throw new ArgumentException(string.Format("OfflineDeposit with Id {0} was not found", id));
            }
            return offlineDeposit;
        }

        public void Seed()
        {
            //todo: need refactoring and join add brand and add brand currency
            Licensees.AddOrUpdate(new Licensee { Id = new Guid("4a557ea9-e6b7-4f1f-aee5-49e170adb7e0"), Name = "Flycow" });

            Brands.AddOrUpdate(brand => brand.Id, new Domain.Payment.Data.Brand
            {
                Id = _brand138Id,
                Name = "138",
                LicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0"),
                LicenseeName = "Flycow",
                BaseCurrencyCode = "CAD",
            });

            Brands.AddOrUpdate(brand => brand.Id, new Domain.Payment.Data.Brand
            {
                Id = _brand831Id,
                Name = "831",
                LicenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0"),
                LicenseeName = "Flycow",
                BaseCurrencyCode = "CAD"
            });

            SaveChanges();

            //todo: until brand base currency change logic appear
            BrandCurrencies.AddOrUpdate(new BrandCurrency { BrandId = _brand138Id, CurrencyCode = "CAD" });
            CurrencyExchanges.AddOrUpdate(
                new CurrencyExchange
                {
                    BrandId = _brand138Id,
                    CurrencyToCode = "CAD",
                    CurrentRate = (decimal)1.0,
                    IsBaseCurrency = true,
                    CreatedBy = "Initializer",
                    DateCreated = DateTime.Now
                });

            BrandCurrencies.AddOrUpdate(new BrandCurrency { BrandId = _brand138Id, CurrencyCode = "CNY" });

            CurrencyExchanges.AddOrUpdate(
                new CurrencyExchange
                {
                    BrandId = _brand138Id,
                    CurrencyToCode = "CNY",
                    CurrentRate = (decimal)4.77,
                    IsBaseCurrency = false,
                    CreatedBy = "Initializer",
                    DateCreated = DateTime.Now
                });

            BrandCurrencies.AddOrUpdate(new BrandCurrency { BrandId = _brand831Id, CurrencyCode = "CAD" });
            CurrencyExchanges.AddOrUpdate(
                new CurrencyExchange
                {
                    BrandId = _brand831Id,
                    CurrencyToCode = "CAD",
                    CurrentRate = (decimal)1.0,
                    IsBaseCurrency = true,
                    CreatedBy = "Initializer",
                    DateCreated = DateTime.Now
                });
            BrandCurrencies.AddOrUpdate(new BrandCurrency { BrandId = _brand831Id, CurrencyCode = "CNY" });

            SaveChanges();

            VipLevels.AddOrUpdate(v => v.Id, new VipLevel()
            {
                Id = new Guid("30e9988c-afed-49a0-be6b-ad60f7a50beb"),
                BrandId = _brand831Id,
                Name = "Gold",
            });

            VipLevels.AddOrUpdate(v => v.Id, new VipLevel()
            {
                Id = new Guid("0447e567-bdc6-4330-979c-5e0984bfb626"),
                BrandId = _brand138Id,
                Name = "Silver",
            });

            VipLevels.AddOrUpdate(v => v.Id, new VipLevel()
            {
                Id = new Guid("541F60EF-AEE7-408B-9B39-90289D49F6AD"),
                BrandId = _brand138Id,
                Name = "Bronze",
            });

            SaveChanges();

            var bank = AddBank("SE45", "Bank of Canada", "Canada", _brand138Id);

            var cadAccountId = new Guid("B6755CB9-8F9A-4EBA-87E0-1ED5493B7534");

            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = cadAccountId,
                    AccountId = "BoC1",
                    AccountName = "John Doe",
                    AccountNumber = "SE45 0583 9825 7466",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "Vancouver",
                    CurrencyCode = "CAD",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            bank = AddBank("GB29", "Canadian Western Bank", "Canada", _brand138Id);

            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = new Guid("973311D5-FBF9-46D2-B0B9-C56E5BBAFDFD"),
                    AccountId = "CWB1",
                    AccountName = "Chris Fowler",
                    AccountNumber = "GB29 NWBK 9268 19",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "Edmonton",
                    CurrencyCode = "CAD",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            bank = AddBank("6016", "California Federal Bank", "California", _brand138Id);

            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = new Guid("E102CCFA-C44D-4EEF-9AA3-E27F327D02E6"),
                    AccountId = "CFB1",
                    AccountName = "Cornelio Bagaria",
                    AccountNumber = "GB29 NWBK 9268 19",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "Province 2",
                    CurrencyCode = "USD",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            bank = AddBank("NWBK", "HSBC", "Great Britain", _brand138Id);

            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = new Guid("86FE5A6D-7053-4279-87D7-9946901DECD3"),
                    AccountId = "HSBC1",
                    AccountName = "Canary Wharf",
                    AccountNumber = "YY24KIHB470915268",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "Province 1",
                    CurrencyCode = "GBP",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            bank = AddBank("70AC", "Hua Xia Bank", "China", _brand138Id);

            var cnyAccountId = new Guid("13672261-70AC-46E3-9E62-9E2E3AB77663");
            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = cnyAccountId,
                    AccountId = "HXB1",
                    AccountName = "Beijing",
                    AccountNumber = "BA3912940494",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "Beijing Municipality",
                    CurrencyCode = "CNY",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            var paymentLevel = new PaymentLevel
            {
                Id = new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"),
                BrandId = bank.BrandId,
                CurrencyCode = "CAD",
                Name = "CADLevel",
                Code = "CADLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer",
                DateActivated = DateTimeOffset.Now,
                ActivatedBy = "Initializer",
                Status = PaymentLevelStatus.Active
            };
            paymentLevel.BankAccounts.Add(BankAccounts.Single(a => a.Id == cadAccountId));
            PaymentLevels.AddOrUpdate(paymentLevel);
            SaveChanges();

            paymentLevel = new PaymentLevel
            {
                Id = new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9"),
                BrandId = bank.BrandId,
                CurrencyCode = "CNY",
                Name = "CNYLevel",
                Code = "CNYLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer",
                DateActivated = DateTimeOffset.Now,
                ActivatedBy = "Initializer",
                Status = PaymentLevelStatus.Active
            };

            paymentLevel.BankAccounts.Add(BankAccounts.Single(a => a.Id == cnyAccountId));
            PaymentLevels.AddOrUpdate(paymentLevel);
            SaveChanges();

            bank = AddBank("56AB", "Vancity", "Canada", _brand831Id);
            cadAccountId = new Guid("D38241CF-9553-4219-8E34-9D0D16294F48");
            BankAccounts.AddOrUpdate(p => p.Id,
                new BankAccount
                {
                    Id = cadAccountId,
                    AccountId = "GH3E",
                    AccountName = "Vancouver",
                    AccountNumber = "CD5432876523",
                    AccountType = "Main",
                    Bank = bank,
                    Branch = "Main",
                    Province = "BC",
                    CurrencyCode = "CAD",
                    Created = DateTime.Now,
                    CreatedBy = "Initializer",
                    Status = BankAccountStatus.Active,
                    Updated = DateTime.Now,
                    UpdatedBy = "Initializer"
                });

            SaveChanges();

            paymentLevel = new PaymentLevel
            {
                Id = new Guid("54A8B43D-B200-43A0-BCB4-4E2623BD5353"),
                BrandId = bank.BrandId,
                CurrencyCode = "CAD",
                Name = "CADVan",
                Code = "CADVan",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer",
                DateActivated = DateTimeOffset.Now,
                ActivatedBy = "Initializer",
                Status = PaymentLevelStatus.Active
            };
            paymentLevel.BankAccounts.Add(BankAccounts.Single(a => a.Id == cadAccountId));
            PaymentLevels.AddOrUpdate(paymentLevel);
            SaveChanges();

            PlayerPaymentLevels.AddOrUpdate(new PlayerPaymentLevel
            {
                PaymentLevel = PaymentLevels.Single(x => x.Id == new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")),
                PlayerId = new Guid("315E8BBA-41AC-41A0-A3C1-C60E5A9CE960")
            });

            PlayerPaymentLevels.AddOrUpdate(new PlayerPaymentLevel
            {
                PaymentLevel = PaymentLevels.Single(x => x.Id == new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")),
                PlayerId = new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05")
            });

            PlayerPaymentLevels.AddOrUpdate(new PlayerPaymentLevel
            {
                PaymentLevel = PaymentLevels.Single(x => x.Id == new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")),
                PlayerId = new Guid("D568A5E8-56AA-4A0E-96BC-D2FBCBFB5D34")
            });

            PlayerPaymentLevels.AddOrUpdate(new PlayerPaymentLevel
            {
                PaymentLevel = PaymentLevels.Single(x => x.Id == new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")),
                PlayerId = new Guid("8D1E5228-CA52-4503-89B8-E65B720C82DE")
            });

            SaveChanges();
        }

        private Bank AddBank(string bankId, string bankName, string country, Guid brandId)
        {
            var bank = Banks.FirstOrDefault(c => c.BankId == bankId);
            if (bank == null)
            {
                bank = new Bank
                {
                    Id = Guid.NewGuid(),
                    BankId = bankId,
                    Name = bankName,
                    BrandId = brandId,
                    CountryCode = country,
                    Created = DateTime.Now,
                    CreatedBy = "initializer"
                };
                Banks.Add(bank);
            }
            return bank;
        }
    }
}
