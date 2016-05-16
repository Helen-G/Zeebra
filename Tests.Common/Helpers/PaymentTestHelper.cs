using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class PaymentTestHelper
    {
        private readonly OfflineDepositCommands _offlineDepositCommands;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly SecurityTestHelper _securityTestHelper;
        private readonly PaymentSettingsCommands _paymentSettingsCommands;
        private readonly BankAccountCommands _bankAccountCommands;
        private readonly PlayerBankAccountCommands _playerBankAccountCommands;
        private readonly ITransferFundCommands _transferFundCommands;
        private readonly BrandCommands _brandCommands;
        private readonly IBrandRepository _brandRepository;

        public PaymentTestHelper(
            OfflineDepositCommands offlineDepositCommands,
            IPaymentRepository paymentRepository,
            IPlayerRepository playerRepository,
            SecurityTestHelper securityTestHelper,
            PaymentSettingsCommands paymentSettingsCommands,
            BankAccountCommands bankAccountCommands,
            PlayerBankAccountCommands playerBankAccountCommands,
            ITransferFundCommands transferFundCommands,
            BrandCommands brandCommands,
            IBrandRepository brandRepository)
        {
            _offlineDepositCommands = offlineDepositCommands;
            _paymentRepository = paymentRepository;
            _playerRepository = playerRepository;
            _securityTestHelper = securityTestHelper;
            _paymentSettingsCommands = paymentSettingsCommands;
            _bankAccountCommands = bankAccountCommands;
            _playerBankAccountCommands = playerBankAccountCommands;
            _transferFundCommands = transferFundCommands;
            _brandCommands = brandCommands;
            _brandRepository = brandRepository;
        }

        /// <summary>
        /// This method adds delays between offline deposit process steps
        /// This allows to process events in correct order
        /// </summary>
        public void MakeDepositSelenium(Guid playerId, decimal depositAmount = 200, string bonusCode = null)
        {
            var offlineDeposit = CreateOfflineDeposit(playerId, depositAmount, bonusCode);
            Thread.Sleep(2000);
            ConfirmOfflineDeposit(offlineDeposit);
            VerifyOfflineDeposit(offlineDeposit, true);
            ApproveOfflineDeposit(offlineDeposit, true);
            Thread.Sleep(1000);
        }

        public void MakeDeposit(string username, decimal depositAmount)
        {
            var playerId = _playerRepository.Players.Single(p => p.Username == username).Id;
            MakeDeposit(playerId, depositAmount);
        }

        public void MakeDeposit(Guid playerId, decimal depositAmount = 200, string bonusCode = null)
        {
            var offlineDeposit = CreateOfflineDeposit(playerId, depositAmount, bonusCode);
            ConfirmOfflineDeposit(offlineDeposit);
            VerifyOfflineDeposit(offlineDeposit, true);
            ApproveOfflineDeposit(offlineDeposit, true);
        }

        public void MakeFundIn(Guid playerId, Guid destinationWalletStructureId, decimal amount, string bonusCode = null)
        {
            _transferFundCommands.AddFund(new TransferFundRequest
            {
                Amount = amount,
                BonusCode = bonusCode,
                PlayerId = playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = destinationWalletStructureId.ToString()
            });
        }

        public OfflineDeposit CreateOfflineDeposit(Guid playerId, decimal depositAmount, string bonusCode = null)
        {
            var player = _playerRepository.Players.Single(p => p.Id == playerId);

            var brandBankAccount =
                _paymentRepository.BankAccounts.FirstOrDefault(
                    ba => ba.Bank.BrandId == player.BrandId && ba.CurrencyCode == player.CurrencyCode);

            var offlineDeposit = _offlineDepositCommands.Submit(new OfflineDepositRequest
            {
                PlayerId = playerId,
                BonusCode = bonusCode,
                Amount = depositAmount,
                BankAccountId = brandBankAccount.Id,
                RequestedBy = _securityTestHelper.CurrentUser.UserName
            });
            offlineDeposit.ReferenceNumber = offlineDeposit.TransactionNumber;
            // TODO: reference number should be populated from Submit method above
            offlineDeposit.Player = _paymentRepository.Players.Single(p => p.Id == playerId);
            return offlineDeposit;
        }

        public void ConfirmOfflineDeposit(OfflineDeposit deposit)
        {
            _offlineDepositCommands.Confirm(new OfflineDepositConfirm
            {
                Amount = deposit.Amount,
                BankId = deposit.BankAccount.Bank.Id,
                OfflineDepositType = DepositMethod.CounterDeposit,
                Id = deposit.Id,
                //PlayerAccountName = deposit.BankAccount.AccountName,
                PlayerAccountName = deposit.Player.GetFullName(),
                PlayerAccountNumber = deposit.BankAccount.AccountNumber,
                ReferenceNumber = deposit.ReferenceNumber,
                TransferType = TransferType.SameBank
            }, new byte[0], new byte[0], new byte[0]);
        }

        public void VerifyOfflineDeposit(OfflineDeposit deposit, bool success)
        {
            if (success)
            {
                _offlineDepositCommands.Verify(deposit.Id, "test verification success");
            }
            else
            {
                _offlineDepositCommands.Unverify(deposit.Id, "test verification fail", UnverifyReasons.D0001);
            }
        }

        public void ApproveOfflineDeposit(OfflineDeposit deposit, bool success, decimal fee = 0)
        {
            if (success)
            {
                _offlineDepositCommands.Approve(new OfflineDepositApprove
                {
                    Id = deposit.Id,
                    Fee = fee,
                    ActualAmount = deposit.Amount - fee,
                    Remark = "test deposit approved"
                });
            }
            else
            {
                _offlineDepositCommands.Reject(deposit.Id, "test deposit rejected");
            }
        }

        public Bank CreateBank(Guid brandId, string countryCode)
        {
            var bank = _paymentRepository
                .Banks
                .SingleOrDefault(b => b.CountryCode == countryCode && b.BrandId == brandId);
            if (bank == null)
            {
                bank = new Bank
                {
                    Id = Guid.NewGuid(),
                    CountryCode = countryCode,
                    Name = "Test Bank",
                    BrandId = brandId
                };
                _paymentRepository.Banks.Add(bank);
                _paymentRepository.SaveChanges();
            }
            return bank;
        }

        public PaymentSettings CreatePaymentSettings(Core.Brand.Data.Brand brand, PaymentType type, bool enable = true)
        {
            var brandBankAccount = _paymentRepository.BankAccounts.SingleOrDefault(ba => ba.Bank.BrandId == brand.Id);

            if (brandBankAccount == null)
            {
                brandBankAccount = CreateBankAccount(brand.Id, brand.DefaultCurrency);
            }

            var model = new SavePaymentSettingsCommand
            {
                Id = Guid.NewGuid(),
                Licensee = brand.Licensee.Id,
                Brand = brand.Id,
                PaymentType = type,
                PaymentMethod = brandBankAccount.Id,
                Currency = brand.BrandCurrencies.First().CurrencyCode,
                VipLevel = brand.DefaultVipLevelId.ToString(),
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 200,
                MaxTransactionPerDay = 10,
                MaxTransactionPerWeek = 20,
                MaxTransactionPerMonth = 30
            };

            var paymentSettingsId = _paymentSettingsCommands.AddSettings(model);

            var ps =
                _paymentRepository.PaymentSettings.Include(x => x.PaymentGateway)
                    .Include(x => x.PaymentGateway.BankAccount)
                    .Single(x => x.Id == paymentSettingsId);

            if (enable)
                _paymentSettingsCommands.Enable(ps.Id, "remark");

            return ps;
        }

        public PaymentSettings CreatePaymentSettings(Core.Brand.Data.Brand brand, PaymentType type, string vipLevel, PaymentSettingsValues settings)
        {
            var brandBankAccount = _paymentRepository.BankAccounts.SingleOrDefault(ba => ba.Bank.BrandId == brand.Id) ??
                                   CreateBankAccount(brand.Id, brand.DefaultCurrency);
            var model = new SavePaymentSettingsCommand
            {
                Id = Guid.NewGuid(),
                Licensee = brand.Licensee.Id,
                Brand = brand.Id,
                PaymentType = type,
                PaymentMethod = brandBankAccount.Id,
                Currency = brand.BrandCurrencies.First().CurrencyCode,
                VipLevel = vipLevel,
                MinAmountPerTransaction = settings.MinAmountPerTransaction,
                MaxAmountPerTransaction = settings.MaxAmountPerTransaction,
                MaxTransactionPerDay = settings.MaxTransactionsPerDay,
                MaxTransactionPerWeek = settings.MaxTransactionsPerWeek,
                MaxTransactionPerMonth = settings.MaxTransactionsPerMonth
            };

            var paymentSettingsId = _paymentSettingsCommands.AddSettings(model);

            var ps =
                _paymentRepository.PaymentSettings.Include(x => x.PaymentGateway)
                    .Include(x => x.PaymentGateway.BankAccount)
                    .Single(x => x.Id == paymentSettingsId);

            _paymentSettingsCommands.Enable(ps.Id, "remark");
            return ps;
        }

        public BankAccount CreateBankAccount(Guid brandId, string currencyCode)
        {
            var bank = _paymentRepository.Banks.Single(b => b.BrandId == brandId);
            var data = new AddBankAccountData
            {
                AccountId = TestDataGenerator.GetRandomString(10),
                AccountName = TestDataGenerator.GetRandomString(10),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Bank = bank.Id,
                Branch = "Branch",
                Currency = currencyCode,
                Province = "Province"
            };

            var bankAccount = _bankAccountCommands.Add(data);
            _bankAccountCommands.Activate(bankAccount.Id, "remark");
            return bankAccount;
        }

        public PaymentLevel CreatePaymentLevel(Guid brandId, string currencyCode)
        {
            var brandBankAccounts = _paymentRepository.Banks.Include(b => b.Accounts).Single(b => b.BrandId == brandId).Accounts;

            var hasDefaultPaymentLevel = false;
            var brand = _brandRepository.Brands.Include(x => x.BrandCurrencies)
                .SingleOrDefault(o => o.Id == brandId);

            if (brand != null)
                hasDefaultPaymentLevel = brand
                    .BrandCurrencies
                    .Any(o => o.BrandId == brandId
                        && o.CurrencyCode == currencyCode
                        && o.DefaultPaymentLevelId.HasValue
                        && o.DefaultPaymentLevelId != Guid.Empty
                        );

            var paymentLevelCode = TestDataGenerator.GetRandomString(3);
            var paymentLevelId = Guid.NewGuid();
            var paymentLevel = new PaymentLevel
            {
                Id = paymentLevelId,
                BrandId = brandId,
                CurrencyCode = currencyCode,
                CreatedBy = _securityTestHelper.CurrentUser.UserName,
                DateCreated = DateTimeOffset.Now,
                Code = "PL-" + paymentLevelCode,
                Name = "PaymentLevel-" + paymentLevelCode,
                EnableOfflineDeposit = true,
                BankAccounts = brandBankAccounts
            };

            if (!hasDefaultPaymentLevel)
                _brandCommands.MakePaymentLevelDefault(paymentLevelId, brandId, currencyCode);

            _paymentRepository.PaymentLevels.Add(paymentLevel);

            _paymentRepository.SaveChanges();

            return paymentLevel;
        }

        public PlayerBankAccount CreatePlayerBankAccount(Guid playerId, bool verify = false)
        {
            var bankAccount = _playerBankAccountCommands.Add(new EditPlayerBankAccountCommand
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Bank = _paymentRepository.Banks.First().Id,
                AccountName = TestDataGenerator.GetRandomString(7),
                AccountNumber = TestDataGenerator.GetRandomString(10, "1234567890"),
                Province = TestDataGenerator.GetRandomString(7),
                City = TestDataGenerator.GetRandomString(7),
                Branch = TestDataGenerator.GetRandomString(7),
                SwiftCode = TestDataGenerator.GetRandomString(7),
                Address = TestDataGenerator.GetRandomString(7)
            });

            if (verify)
            {
                VerifyPlayerBankAccount(bankAccount.Id);
            }

            return bankAccount;
        }

        public Currency CreateCurrency(string code, string name)
        {
            var currency = _paymentRepository.Currencies.SingleOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Code = code,
                    CreatedBy = "test",
                    DateCreated = DateTimeOffset.UtcNow,
                    Name = name,
                    Remarks = "remark"
                    
                };
                _paymentRepository.Currencies.Add(currency);
            }
            return currency;
        }

        private void VerifyPlayerBankAccount(Guid bankAccountId)
        {
            _playerBankAccountCommands.Verify(bankAccountId, "remark");
        }
    }
}