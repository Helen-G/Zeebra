using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.Mail;
using AFT.RegoV2.Infrastructure.Sms;
using AFT.RegoV2.MemberApi;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using VipLevel = AFT.RegoV2.Core.Player.Data.VipLevel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared.Constants;
using AFT.RegoV2.Tests.Common.Helpers;
using BrandCurrency = AFT.RegoV2.Core.Brand.Data.BrandCurrency;
using Licensee = AFT.RegoV2.Core.Brand.Data.Licensee;

namespace AFT.RegoV2.Tests.Unit.Player
{

    public class TestStartup : Startup
    {
        public static IUnityContainer Container;
        protected override IUnityContainer GetUnityContainer()
        {
            return Container;
        }
    }

    public class MockAuthServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);

            identity.AddClaim(new Claim("username", context.UserName));
            identity.AddClaim(new Claim("playerId", Guid.NewGuid().ToString()));


            context.Validated(identity);
            context.Request.Context.Authentication.SignIn(identity);

            return Task.FromResult<object>(null);
        }
    }

    internal abstract class PlayerServiceTestsBase : AdminWebsiteUnitTestsBase
    {
        protected MemberApiProxy PlayerWebservice { get; set; }
        protected FakeBrandRepository FakeBrandRepository { get; set; }
        protected FakeBonusRepository FakeBonusRepository { get; set; }
        protected FakePlayerRepository FakePlayerRepository { get; set; }
        protected FakePaymentRepository FakePaymentRepository { get; set; }
        protected FakeContentsRepository FakeContentsRepository { get; set; }
        protected FakeEventRepository FakeEventRepository { get; set; }
        protected FakeSecurityRepository FakeSecurityRepository { get; set; }
        protected PermissionService PermissionService { get; set; }

        private IDisposable _webServer;

        private string _testServerUri;

        public override void BeforeEach()
        {

            _testServerUri = ConfigurationManager.AppSettings["TestServerUri"];

            base.BeforeEach();


            Container.RegisterType<SmsNotifier>();
            Container.RegisterType<EmailNotifier>();

            Container.RegisterType<PlayerCommands>();
            Container.RegisterType<PlayerQueries>();


            FakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            FakeBonusRepository = Container.Resolve<FakeBonusRepository>();
            FakePlayerRepository = Container.Resolve<FakePlayerRepository>();
            FakePaymentRepository = Container.Resolve<FakePaymentRepository>();
            FakeEventRepository = Container.Resolve<FakeEventRepository>();
            FakeSecurityRepository = Container.Resolve<FakeSecurityRepository>();
            FakeContentsRepository = Container.Resolve<FakeContentsRepository>();
            PermissionService = Container.Resolve<PermissionService>();

            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                FakeBrandRepository.Countries.Add(new Country { Code = TestDataGenerator.CountryCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                FakeBrandRepository.Currencies.Add(new Currency { Code = TestDataGenerator.CurrencyCodes[i] });
            }

            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                FakeBrandRepository.Cultures.Add(new Culture { Code = TestDataGenerator.CultureCodes[i] });
            }

            var brandId = new Guid("00000000-0000-0000-0000-000000000138");
            var brand = new Core.Brand.Data.Brand { Id = brandId, Name = "138", Status = BrandStatus.Active };
            for (int i = 0; i < TestDataGenerator.CurrencyCodes.Length; i++)
            {
                var currencyCode = TestDataGenerator.CurrencyCodes[i];

                brand.BrandCurrencies.Add(new BrandCurrency
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CurrencyCode = currencyCode,
                    Currency = FakeBrandRepository.Currencies.Single(x => x.Code == currencyCode),
                    DefaultPaymentLevelId = currencyCode == "CAD"
                        ? new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33")
                        : new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9")
                });

            }
            for (int i = 0; i < TestDataGenerator.CountryCodes.Length; i++)
            {
                var countryCode = TestDataGenerator.CountryCodes[i];

                brand.BrandCountries.Add(new BrandCountry
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CountryCode = countryCode,
                    Country = FakeBrandRepository.Countries.Single(x => x.Code == countryCode)
                });
            }
            for (int i = 0; i < TestDataGenerator.CultureCodes.Length; i++)
            {
                var cultureCode = TestDataGenerator.CultureCodes[i];

                brand.BrandCultures.Add(new BrandCulture
                {
                    BrandId = brand.Id,
                    Brand = brand,
                    CultureCode = cultureCode,
                    Culture = FakeBrandRepository.Cultures.Single(x => x.Code == cultureCode)
                });
            }
            var walletTemplate = new WalletTemplate()
            {
                Brand = brand,
                Id = Guid.NewGuid(),
                IsMain = true,
                Name = "Main wallet",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            brand.WalletTemplates.Add(walletTemplate);
            brand.DefaultCulture = brand.BrandCultures.First().Culture.Code;
            brand.DefaultCurrency = brand.BrandCurrencies.First().Currency.Code;
            var vipLevel = new Core.Brand.Data.VipLevel { Name = "Standard", BrandId = brandId };
            brand.DefaultVipLevelId = vipLevel.Id;
            brand.DefaultVipLevel = vipLevel;

            FakeBrandRepository.WalletTemplates.Add(walletTemplate);
            var playerVipLevel = new VipLevel
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                BrandId = brandId
            };
            brand.DefaultVipLevelId = playerVipLevel.Id;

            FakeBrandRepository.Brands.Add(brand);

            var playerBrand = new Core.Player.Data.Brand { Id = brand.Id };
            FakePlayerRepository.Brands.Add(playerBrand);

            FakePlayerRepository.VipLevels.Add(playerVipLevel);
            playerBrand.DefaultVipLevelId = playerVipLevel.Id;
            FakePlayerRepository.SaveChanges();

            foreach (var questionid in TestDataGenerator.SecurityQuestions)
            {
                FakePlayerRepository.SecurityQuestions.Add(new SecurityQuestion
                {
                    Id = new Guid(questionid),
                    Question = TestDataGenerator.GetRandomString()
                });
            }

            FakeBonusRepository.Brands.Add(new Core.Bonus.Data.Brand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138")
            });

            Container.Resolve<FakeGameRepository>().Brands.Add(new Core.Game.Data.Brand
            {
                Id = new Guid("00000000-0000-0000-0000-000000000138"),
                TimezoneId = TestDataGenerator.GetRandomTimeZone().Id
            });

            var bank = new Bank
            {
                Id = Guid.NewGuid(),
                BankId = "SE45",
                Name = "Bank of Canada",
                BrandId = brandId,
                CountryCode = "Canada",
                Created = DateTime.Now,
                CreatedBy = "initializer"
            };
            FakePaymentRepository.Banks.Add(bank);

            var cadAccountId = new Guid("B6755CB9-8F9A-4EBA-87E0-1ED5493B7534");
            FakePaymentRepository.BankAccounts.Add(
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
                }
                );

            bank = new Bank
            {
                Id = Guid.NewGuid(),
                BankId = "70AC",
                Name = "Hua Xia Bank",
                BrandId = brandId,
                CountryCode = "China",
                Created = DateTime.Now,
                CreatedBy = "initializer"
            };
            FakePaymentRepository.Banks.Add(bank);

            var cnyAccountId = new Guid("13672261-70AC-46E3-9E62-9E2E3AB77663");
            FakePaymentRepository.BankAccounts.Add(
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
                }
                );

            var paymentLevel = new PaymentLevel
            {
                Id = new Guid("E1E600D4-0729-4D5C-B93E-085A94B55B33"),
                BrandId = brandId,
                CurrencyCode = "CAD",
                Name = "CADLevel",
                Code = "CADLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer"
            };
            paymentLevel.BankAccounts.Add(FakePaymentRepository.BankAccounts.Single(a => a.Id == cadAccountId));
            FakePaymentRepository.PaymentLevels.Add(paymentLevel);

            paymentLevel = new PaymentLevel
            {
                Id = new Guid("1ED97A2B-EBA2-4B68-A70C-18A7070908F9"),
                BrandId = brandId,
                CurrencyCode = "CNY",
                Name = "CNYLevel",
                Code = "CNYLevel",
                EnableOfflineDeposit = true,
                DateCreated = DateTimeOffset.Now,
                CreatedBy = "Initializer"
            };
            paymentLevel.BankAccounts.Add(FakePaymentRepository.BankAccounts.Single(a => a.Id == cnyAccountId));
            FakePaymentRepository.PaymentLevels.Add(paymentLevel);

            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                AllowedBrandCount = 1,
                Status = LicenseeStatus.Active
            };

            FakeBrandRepository.Licensees.Add(licensee);
            FakeBrandRepository.SaveChanges();

            foreach (var culture in FakeBrandRepository.Cultures)
            {
                FakeContentsRepository.Languages.Add(new Core.Content.Data.Language
                {
                    Code = culture.Code,
                    Name = culture.Name
                });
            }

            foreach (var thisBrand in FakeBrandRepository.Brands.Include(x => x.BrandCultures.Select(y => y.Culture)))
            {
                FakeContentsRepository.Brands.Add(new Core.Content.Data.Brand
                {
                    Id = thisBrand.Id,
                    Name = thisBrand.Name,
                    Languages = thisBrand.BrandCultures.Select(x => new Core.Content.Data.Language
                    {
                        Code = x.Culture.Code,
                        Name = x.Culture.Name
                    }).ToList()
                });
            }

            foreach (var thisPlayer in FakePlayerRepository.Players)
            {
                FakeContentsRepository.Players.Add(new Core.Content.Data.Player
                {
                    Id = thisPlayer.Id,
                    Username = thisPlayer.Username,
                    FirstName = thisPlayer.FirstName,
                    LastName = thisPlayer.LastName,
                    Email = thisPlayer.Email,
                    Language = FakeContentsRepository.Languages.Single(x => x.Code == thisPlayer.CultureCode),
                    Brand = FakeContentsRepository.Brands.Single(x => x.Id == thisPlayer.BrandId)
                });
            }

            FakeContentsRepository.SaveChanges();

            var securityHelper = Container.Resolve<SecurityTestHelper>();
            securityHelper.PopulatePermissions();

            var licenseeIds = new[] { licensee.Id };
            var brandIds = new[] { brand.Id };

            const string superAdminUsername = "SuperAdmin";
            const string superAdminPassword = "SuperAdmin";

            var userId = RoleIds.SuperAdminId;
            var role = new Role
            {
                Id = userId,
                Code = "SuperAdmin",
                Name = "SuperAdmin",
                CreatedDate = DateTime.UtcNow,
                Permissions = PermissionService.GetPermissions()
                    .Select(p => new RolePermission { PermissionId = p.Id, RoleId = userId }).ToList()
            };

            role.SetLicensees(licenseeIds);

            var user = new User
            {
                Id = userId,
                Username = superAdminUsername,
                FirstName = superAdminUsername,
                LastName = superAdminUsername,
                Status = UserStatus.Active,
                Description = superAdminUsername,
                PasswordEncrypted = PasswordHelper.EncryptPassword(userId, superAdminPassword),
                Role = role
            };
            user.SetLicensees(licenseeIds);

            foreach (var licenseeId in licenseeIds)
            {
                user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    UserId = user.Id,
                    LicenseeId = licenseeId,
                    User = user
                });
            }

            user.SetAllowedBrands(brandIds);

            foreach (var item in brandIds)
            {
                user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    UserId = user.Id,
                    BrandId = item,
                    User = user
                });
            }

            FakeSecurityRepository.Users.AddOrUpdate(user);

            FakeSecurityRepository.SaveChanges();

            securityHelper.SignInSuperAdmin();

            TestStartup.Container = Container;
            _webServer = WebApp.Start<TestStartup>(_testServerUri);

            PlayerWebservice = new MemberApiProxy(_testServerUri);

        }

        protected async Task<RegisterRequest> RegisterPlayer(bool doLogin = true)
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();

            await PlayerWebservice.RegisterAsync(registrationData);

            if (doLogin)
            {
                await PlayerWebservice.Login(new LoginRequest
                {
                    Username = registrationData.Username,
                    Password = registrationData.Password,
                    BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                    IPAddress = "::1",
                    RequestHeaders = new Dictionary<string, string>()
                });
            }
            return registrationData;
        }

        public override void AfterEach()
        {
            _webServer.Dispose();
        }
    }
}