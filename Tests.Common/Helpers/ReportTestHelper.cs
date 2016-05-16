using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.BoundedContexts.Brand;
using AFT.RegoV2.BoundedContexts.Brand.ApplicationServices;
using AFT.RegoV2.BoundedContexts.Brand.Data;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.GameServerIntegration;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.GSI.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Core.Shared.Data;
using AFT.RegoV2.Core.Wallet.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Brand.Validators;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Brand = AFT.RegoV2.BoundedContexts.Brand.Data.Brand;
using PlayerData = AFT.RegoV2.Core.Common.Data.Player;
using VipLevel = AFT.RegoV2.BoundedContexts.Brand.Data.VipLevel;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class ReportTestHelper
    {
        private readonly BrandCommands _brandCommands;
        private readonly BrandQueries _brandQueries;
        private readonly IBrandRepository _brandRepository;
        private readonly FakeBus _bus;
        private readonly OfflineDepositCommands _depositCommands;
        private readonly IGameServerIntegrationRepository _gsiRepository;
        private readonly PaymentQueries _paymentQueries;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPermissionProvider _permissionProvider;
        private readonly PlayerCommands _playerCommands;
        private readonly PlayerQueries _playerQueries;
        private readonly RoleService _roleService;
        private readonly ISharedData _sharedData;
        private readonly UserService _userService;
        private readonly WalletCommands _walletWalletCommands;

        public ReportTestHelper(
            BrandCommands brandCommands,
            BrandQueries brandQueries,
            IBrandRepository brandRepository,
            FakeBus bus,
            OfflineDepositCommands depositCommands,
            IGameServerIntegrationRepository gameServerIntegrationRepository,
            PaymentQueries paymentQueries,
            IPaymentRepository paymentRepository,
            IPermissionProvider permissionProvider,
            PlayerCommands playerCommands,
            PlayerQueries playerQueries,
            RoleService roleService,
            ISharedData sharedData,
            UserService userService,
            WalletCommands walletWalletCommands
            )
        {
            _brandCommands = brandCommands;
            _brandQueries = brandQueries;
            _brandRepository = brandRepository;
            _bus = bus;
            _depositCommands = depositCommands;
            _paymentQueries = paymentQueries;
            _paymentRepository = paymentRepository;
            _permissionProvider = permissionProvider;
            _gsiRepository = gameServerIntegrationRepository;
            _playerCommands = playerCommands;
            _playerQueries = playerQueries;
            _roleService = roleService;
            _sharedData = sharedData;
            _userService = userService;
            _walletWalletCommands = walletWalletCommands;
        }

        public void SignInUser(User user)
        {
            _sharedData.SetUser(user);
        }

        public Licensee CreateLicensee()
        {
            var name = TestDataGenerator.GetRandomString(5);
            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                Name = name,
                CompanyName = name + " Inc.",
                Email = TestDataGenerator.GetRandomEmail(),
                ContractStart = DateTime.Now.Date,
                ContractEnd = DateTime.Now.Date.AddMonths(1),
                AllowedBrandCount = 10,
                Status = LicenseeStatus.Inactive
            };
            _brandRepository.Licensees.Add(licensee);
            _bus.Publish(new LicenseeCreated(licensee));
            return licensee;
        }

        public Role CreateRole()
        {
            var role = new Role
            {
                Code = "Role-" + TestDataGenerator.GetRandomString(5),
                Name = "Role-" + TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(),
                Operations = _permissionProvider.CreateRoleOperations(_permissionProvider.GetOperations()).ToList()
            };
            return _roleService.CreateRole(role, null);
        }

        public User CreateUser(Role role, Licensee licensee, IEnumerable<Brand> brands = null)
        {
            var userName = "User-" + TestDataGenerator.GetRandomString(5);
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = userName,
                FirstName = userName,
                LastName = userName,
                PasswordEncrypted = PasswordHelper.EncryptPassword(userId, TestDataGenerator.GetRandomString()),
                Language = "English",
                Status = UserStatus.Active
            };
            user.SetLicensees(new [] { licensee.Id });
            user.SetAllowedBrands(brands != null ? brands.Select(b => b.Id) : null);
            return _userService.CreateUser(user, role.Id);
        }

        public Brand CreateBrand(
            Licensee licensee = null,
            Country country = null,
            CultureCode culture = null,
            Currency currency = null
            )
        {
            licensee = licensee ?? _brandRepository.Licensees.FirstOrDefault() ?? CreateLicensee();
            country = country ?? CreateCountry("CA", "Canada");
            culture = culture ?? CreateCulture("en-CA", "English (Canada)");
            currency = currency ?? CreateCurrency("CAD", "Canadian Dollar");            
            var brandName = TestDataGenerator.GetRandomString(20);
            var brandCode = TestDataGenerator.GetRandomString(20);
            string playerPrefix;

            do
            {
                playerPrefix = TestDataGenerator.GetRandomString(3);
            } while (_brandRepository.Brands.Any(x => x.PlayerPrefix == playerPrefix && x.Licensee.Id == licensee.Id));

            _brandCommands.AddBrand(new AddBrandData
            {
                Code = brandCode,
                EnablePlayerPrefix = true,
                InternalAccounts = 10,
                Licensee = licensee.Id,
                Name = brandName,
                PlayerPrefix = playerPrefix,
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Type = BrandType.Credit,
                PlayerActivationMethod = PlayerActivationMethod.Automatic
            });
            var brand = _brandQueries.GetBrands().Single(b => b.Name == brandName);
            var vipLevel = _brandRepository.VipLevels.FirstOrDefault(vl => vl.IsDefault && vl.Brand.Id == brand.Id) 
                ?? CreateVipLevel(brand);

            brand.Countries.Add(country);
            brand.Cultures.Add(culture);
            brand.DefaultCulture = culture;
            brand.Currencies.Add(currency);
            brand.DefaultCurrency = currency.Code;
            brand.VipLevels.Add(_brandRepository.VipLevels.FirstOrDefault(vl => vl.IsDefault) ?? CreateVipLevel(brand));
            CreatePaymentLevel(brand, currency);
			CreateWallet(brand);
            return brand;
        }

		private void CreateWallet(Brand brand)
		{
			var walletTemplate = new WalletTemplate()
			{
				Brand = brand,
				Name = "Main Wallet",
				Id = Guid.NewGuid(),
				IsMain = true,
                CreatedBy = _sharedData.User.UserId,
				DateCreated = DateTimeOffset.UtcNow
			};
			brand.WalletTemplates.Add(walletTemplate);
			_brandRepository.WalletTemplates.Add(walletTemplate);
			_brandRepository.SaveChanges();
		}

        public Country CreateCountry(string code, string name)
        {
            var country = _brandRepository.Countries.SingleOrDefault(c => c.Code == code);
            if (country == null)
            {
                country = new Country
                {
                    Code = code,
                    Name = name
                };
                _brandRepository.Countries.Add(country);
            }
            return country;
        }

        public Currency CreateCurrency(string code, string name)
        {
            var currency = _brandRepository.Currencies.SingleOrDefault(c => c.Code == code);
            if (currency == null)
            {
                currency = new Currency
                {
                    Code = code,
                    Name = name
                };
                _brandRepository.Currencies.Add(currency);
            }
            return currency;
        }

        public CultureCode CreateCulture(string code, string name)
        {
            var culture = _brandRepository.CultureCodes.SingleOrDefault(c => c.Code == code);
            if (culture == null)
            {
                culture = new CultureCode
                {
                    Code = code,
                    Name = name,
                    NativeName = name
                };
                _brandRepository.CultureCodes.Add(culture);
                _bus.Publish(new LanguageCreated(culture));
            }
            return culture;
        }

        public PlayerData CreatePlayer(bool isActive = true)
        {
            var isMale = new Random().Next(2) == 1;
            var brand = _brandRepository.Brands.First();
            var password = TestDataGenerator.GetRandomString(12);


            var playerId = _playerCommands.Register(new RegistrationData
            {
                FirstName = TestDataGenerator.GetRandomString(),
                LastName = TestDataGenerator.GetRandomString(10),
                Email = TestDataGenerator.GetRandomEmail(),
                PhoneNumber = TestDataGenerator.GetRandomString(12, TestDataGenerator.NumericChars),
                MailingAddressLine1 = "Address Line 1",
                MailingAddressLine2 = "Address Line 2",
                MailingAddressLine3 = "Address Line 3",
                MailingAddressLine4 = "Address Line 4",
                MailingAddressCity = "Test City",
                MailingAddressPostalCode = TestDataGenerator.GetRandomString(5, TestDataGenerator.NumericChars),
                PhysicalAddressLine1 = "Physical Address Line 1",
                PhysicalAddressLine2 = "Physical Address Line 2",
                PhysicalAddressLine3 = "Physical Address Line 3",
                PhysicalAddressLine4 = "Physical Address Line 4",
                PhysicalAddressCity = "Physical Test City",
                PhysicalAddressPostalCode = TestDataGenerator.GetRandomString(5, TestDataGenerator.NumericChars),
                CountryCode = brand.Countries.First().Code,
                CurrencyCode = brand.DefaultCurrency,
                CultureCode = brand.DefaultCulture.Code,
                Username = TestDataGenerator.GetRandomString(12),
                Password = password,
                PasswordConfirm = password,
                DateOfBirth = TestDataGenerator.GetDateOfBirthOver18().ToString("yyyy-MM-dd"),
                BrandId = brand.Id.ToString(),
                Gender = isMale ? Gender.Male.ToString() : Gender.Female.ToString(),
                Title = isMale ? Title.Mr.ToString() : Title.Mrs.ToString(),
                ContactPreference = ContactMethod.Phone.ToString(),
                SecurityQuestionId = TestDataGenerator.GetRandomSecurityQuestion(),
                SecurityAnswer = "Security Answer " + TestDataGenerator.GetRandomString(),
                AccountStatus = isActive ? AccountStatus.Active.ToString() : AccountStatus.Inactive.ToString(),
                IdStatus = "Verified"
            });


            return _playerQueries.GetPlayer(playerId);
        }

        public PaymentLevelDTO CreatePaymentLevel()
        {
            var level = new PaymentLevel();
            level.Id = Guid.NewGuid();
            _paymentRepository.PaymentLevels.Add(level);
            var dto = new PaymentLevelDTO
            {
                Id = level.Id,
            };
            return dto;
        }

        public GameEndpoint CreateGame(string name)
        {
            var game = new GameEndpoint
            {
                Id = Guid.NewGuid(),
                Name = name,
                Url = "/Game/Index"
            };
            _gsiRepository.GameEndpoints.Add(game);
            return game;
        }

        public void DepositFunds(Guid playerId, decimal depositAmount, Guid? bonusRedemptionId = null)
        {
            _walletWalletCommands.Deposit(playerId, depositAmount);
        }

        public OfflineDeposit CreateOfflineDeposit(PlayerData player, decimal amount)
        {
            var bankAccount = CreateBankAccount(player);
            var deposit = _depositCommands.Submit(new OfflineDepositRequest
            {
                PlayerId = player.Id,
                Amount = amount,
                BankAccountId = bankAccount.Id,
                RequestedBy = "Operator 1"
            });
            deposit.Player = _paymentQueries.GetPlayer(player.Id);
            return deposit;
        }

        private BankAccount CreateBankAccount(PlayerData player)
        {
            var bank = CreateBank();
            var bankAccount = new BankAccount
            {
                Id = Guid.NewGuid(),
                CurrencyCode = player.CurrencyCode,
                Status = BankAccountStatus.Active,
                AccountId = "BoC2",
                AccountName = player.GetFullName(),
                AccountNumber = "SE46 0583 9825 7466",
                AccountType = "Main",
                Bank = bank,
                Branch = "Main Branch",
                Province = "Vancouver"
            };
            _paymentRepository.BankAccounts.Add(bankAccount);
            return bankAccount;
        }

        private Bank CreateBank()
        {
            var country = CreateCountry("CA", "Canada");
            var bank = new Bank
            {
                Id = Guid.NewGuid(),
                CountryCode = country.Code,
                Name = "Test Bank"
            };
            _paymentRepository.Banks.Add(bank);
            return bank;
        }

        public void ConfirmOfflineDeposit(OfflineDeposit deposit)
        {
            _depositCommands.Confirm(new OfflineDepositConfirm
            {
                Amount = deposit.Amount,
                BankId = deposit.BankAccount.Bank.Id,
                DepositMethod = DepositMethod.CounterDeposit,
                Id = deposit.Id,
                PlayerAccountName = deposit.BankAccount.AccountName,
                PlayerAccountNumber = deposit.BankAccount.AccountNumber,
                ReferenceNumber = deposit.ReferenceNumber,
                TransferType = TransferType.SameBank
            }, new byte[0], new byte[0], new byte[0]);
        }

        public void VerifyOfflineDeposit(OfflineDeposit deposit, bool success)
        {
            const string name = "Operator 2";
            if (success)
            {
                _depositCommands.Verify(deposit.Id, name, "test verification success");
            }
            else
            {
                _depositCommands.Unverify(deposit.Id, name, "test verification fail");
            }
        }

        public void ApproveOfflineDeposit(OfflineDeposit deposit, bool success, decimal fee = 0)
        {
            const string name = "Operator 3";
            if (success)
            {
                _depositCommands.Approve(new OfflineDepositApprove
                {
                    Id = deposit.Id,
                    Fee = fee,
                    ActualAmount = deposit.Amount - fee,
                    Remark = "test deposit approved"
                }, name);
            }
            else
            {
                _depositCommands.Reject(deposit.Id, name, "test deposit rejected");
            }
        }

        public VipLevel CreateVipLevel(Brand brand, int limitCount = 0)
        {
            var vipLevelName = TestDataGenerator.GetRandomString();
            int rank;
            do
            {
                rank = TestDataGenerator.GetRandomNumber(100);
            } while (_brandRepository.VipLevels.Any(vl => vl.Rank == rank));
            var limits = new List<VipLevelLimitViewModel>();
            for (var i = 0; i < limitCount; i++)
            {
                var gameServer = CreateGameServer();
                var betLimit = CreateBetLevel(gameServer, brand);
                limits.Add(new VipLevelLimitViewModel
                {
                    Id = Guid.NewGuid(),
                    CurrencyCode = brand.DefaultCurrency,
                    GameServerId = gameServer.Id,
                    BetLimitId = betLimit.Id
                });
            }
            var newVipLevel = new VipLevelViewModel
            {
                Id = Guid.NewGuid(),
                Name = vipLevelName,
                Code = vipLevelName.Remove(3),
                Brand = brand.Id,
                Rank = rank,
                IsDefault = _brandRepository.VipLevels.All(vl => !vl.IsDefault),
                Limits = limits
            };
            _brandCommands.AddVipLevel(newVipLevel, _sharedData.User.UserName);
            var vipLevel = _brandQueries.GetAllVipLevels().Single(x => x.Code == newVipLevel.Code);
            return vipLevel;
        }

        public GameServer CreateGameServer()
        {
            var gameServer = new GameServer
            {
                Id = Guid.NewGuid(),
                Name = TestDataGenerator.GetRandomString(),
                IsActive = true,
                Endpoints = new List<GameEndpoint>
                {
                    CreateGame(TestDataGenerator.GetRandomString())
                }
            };
            _gsiRepository.GameServers.Add(gameServer);
            return gameServer;
        }

        public BetLevel CreateBetLevel(GameServer gameServer, Brand brand)
        {
            var betLevel = new BetLevel
            {
                Id = Guid.NewGuid(),
                GameServerId = gameServer.Id,
                BrandId = brand.Id,
                Code = new Random().Next(100000).ToString(),
            };
            _gsiRepository.BetLimits.Add(betLevel);
            return betLevel;
        }

        public PaymentLevel CreatePaymentLevel(
            Brand brand,
            Currency currency)
        {
            var paymentLevelCode = TestDataGenerator.GetRandomString(3);
            var paymentLevel = new PaymentLevel
            {
                Id = Guid.NewGuid(),
                BrandId = brand.Id,
                CurrencyCode = currency.Code,
                CreatedBy = _sharedData.User.UserName,
                DateCreated = DateTimeOffset.Now,
                Code = "PL-" + paymentLevelCode,
                Name = "PaymentLevel-" + paymentLevelCode,
                EnableOfflineDeposit = true,
                IsDefault = true
            };
            _paymentRepository.PaymentLevels.Add(paymentLevel);
            return paymentLevel;
        }

        public string UserName
        {
            get { return _sharedData.User.UserName; }
        }
    }
}