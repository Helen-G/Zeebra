using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Commands;
using AutoMapper;
using ServiceStack.Validation;
using Player = AFT.RegoV2.Core.Payment.Data.Player;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PlayerManagerController : BaseController
    {
        private readonly PlayerCommands _commands;
        private readonly PlayerQueries _queries;
        private readonly BrandQueries _brandQueries;
        private readonly PaymentQueries _paymentQueries;
        private readonly PlayerBankAccountCommands _playerBankAccountCommands;
        private readonly UserService _userService;

        public PlayerManagerController(
            PlayerCommands commands,
            PlayerQueries queries,
            BrandQueries brandQueries,
            PaymentQueries paymentQueries,
            PlayerBankAccountCommands playerBankAccountCommands, 
            UserService userService)
        {
            if (commands == null) throw new ArgumentNullException("commands");
            _commands = commands;
            _queries = queries;
            _brandQueries = brandQueries;
            _paymentQueries = paymentQueries;
            _playerBankAccountCommands = playerBankAccountCommands;
            _userService = userService;
        }

        static PlayerManagerController()
        {
            Mapper.CreateMap<AddPlayerCommand, RegistrationData>()
                .ForMember(x => x.BrandId, y => y.MapFrom(z => z.Brand))
                .ForMember(x => x.CountryCode, y => y.MapFrom(z => z.Country))
                .ForMember(x => x.CultureCode, y => y.MapFrom(z => z.Culture))
                .ForMember(x => x.CurrencyCode, y => y.MapFrom(z => z.Currency));
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            return new JsonResult
            {
                Data = SearchData(searchPackage),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult GetAddPlayerData()
        {
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            var licensees = _brandQueries.GetLicensees()
                .Where(x => 
                    x.Brands.Any(y => y.Status == BrandStatus.Active) && 
                    licenseeFilterSelections.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name});

            var response = new
            {
                Licensees = licensees,
                Genders = Enum.GetNames(typeof (Gender)),
                Titles = Enum.GetNames(typeof (Title)),
                AccountStatuses =
                    new[]
                    {
                        Enum.GetName(typeof (AccountStatus), AccountStatus.Active),
                        Enum.GetName(typeof (AccountStatus), AccountStatus.Inactive)
                    },
                IdStatuses = Enum.GetNames(typeof (IdStatus)),
                ContactMethods = Enum.GetNames(typeof (ContactMethod)).OrderBy(x => x),
            };

            return this.Success(response);
        }

        public ActionResult GetAddPlayerBrands(Guid licenseeId)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var brands = _brandQueries.GetBrands()
                .Where(x =>
                    x.Status == BrandStatus.Active &&
                    x.Licensee.Id == licenseeId &&
                    brandFilterSelections.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new {x.Id, x.Name, x.PlayerPrefix});

            return this.Success(new {brands});
        }

        public ActionResult GetAddPlayerBrandData(Guid brandId)
        {
            var brand = _brandQueries.GetBrandOrNull(brandId);
            var paymentLevels = _paymentQueries.GetPaymentLevels().Where(x => x.BrandId == brandId);

            var currencies = brand.BrandCurrencies
                .Where(x => paymentLevels.Any(y => y.CurrencyCode == x.CurrencyCode && y.IsDefault))
                .OrderBy(x => x.CurrencyCode)
                .Select(x => x.CurrencyCode);

            var countries = brand.BrandCountries
                .OrderBy(x => x.CountryCode)
                .Select(x => x.CountryCode);

            var cultures = brand.BrandCultures
                .OrderBy(x => x.CultureCode)
                .Select(x => x.CultureCode);

            var response = new
            {
                Countries = countries,
                Cultures = cultures,
                Currencies = currencies
            };

            return this.Success(response);
        }

        public string GetPaymentLevels(Guid brandId, string currency)
        {
            var paymentLevels = _paymentQueries.GetPaymentLevels()
                .Where(l => l.BrandId == brandId && l.CurrencyCode == currency)
                .Select(l => new {l.Id, l.Name, l.IsDefault});

            return SerializeJson(new
            {
                PaymentLevels = paymentLevels
            });
        }

        public string GetVipLevels(Guid brandId)
        {
            return SerializeJson(new
            {
                VipLevels = _queries.VipLevels.Where(x => x.BrandId == brandId).OrderBy(x => x.Code)
            });
        }

        public string ChangeVipLevel(ChangeVipLevelCommand command)
        {
            if (ModelState.IsValid == false)
            {
                return ErrorResponse();
            }
            try
            {
                _commands.ChangeVipLevel(command.PlayerId, command.NewVipLevel, command.Remarks);
                return SerializeJson(new {Result = "success"});
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponse(e);
            }
            catch (Exception exception)
            {
                return SerializeJson(new {Result = "failed", Data = exception.Message});
            }
        }

        public string SendNewPassword(SendNewPasswordCommand command)
        {
            if (ModelState.IsValid == false)
            {
                return ErrorResponse();
            }
            try
            {
                _commands.SendNewPassword(new SendNewPasswordData
                {
                    PlayerId = command.PlayerId,
                    NewPassword = command.NewPassword,
                    SendBy = command.SendBy == "Email" ? SendBy.Email : SendBy.Sms
                });

                return SerializeJson(new {Result = "success"});
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponse(e);
            }
            catch (Exception exception)
            {
                return SerializeJson(new {Result = "failed", Data = exception.Message});
            }
        }

        [HttpPost]
        public dynamic Add(AddPlayerCommand command)
        {
            try
            {
                var playerData = Mapper.DynamicMap<RegistrationData>(command);
                playerData.IsRegisteredFromAdminSite = true;
                _commands.Register(playerData);

                return this.Success();
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponse(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public string GetPlayerForBankAccount(Guid id)
        {
            var player = _paymentQueries.GetPlayer(id);
            if (player == null)
            {
                return SerializeJson(new {Result = "failed"});
            }
            return SerializeJson(new
            {
                Result = "success",
                Player = SelectPlayerDataForBankAccount(player)
            });
        }

        public string GetBankAccount(Guid id)
        {
            var bankAccount =
                _paymentQueries.GetPlayerBankAccounts()
                    .SingleOrDefault(x => x.Id == id);
            return bankAccount == null
                ? SerializeJson(new {Result = "failed"})
                : SerializePlayerAndBankAccount(bankAccount.Player, bankAccount);
        }

        public string GetCurrentBankAccount(Guid playerId)
        {
            var player = _paymentQueries.GetPlayerWithBank(playerId);
            return player == null
                ? SerializeJson(new {Result = "failed"})
                : SerializePlayerAndBankAccount(player, player.CurrentBankAccount);
        }

        private string SerializePlayerAndBankAccount(Player player, PlayerBankAccount bankAccount)
        {
            object bankAccountSent = null;
            if (bankAccount != null)
            {
                var bankAcccountTime = bankAccount.Updated ?? bankAccount.Created;
                var bankTime = bankAccount.Bank.Updated ?? bankAccount.Bank.Created;

                bankAccountSent = new
                {
                    bankAccount.Id,
                    Bank = bankAccount.Bank.Id,
                    BankName = bankAccount.Bank.Name,
                    bankAccount.Province,
                    bankAccount.City,
                    bankAccount.Branch,
                    bankAccount.SwiftCode,
                    bankAccount.Address,
                    bankAccount.AccountName,
                    bankAccount.AccountNumber,
                    bankAccount.Status,
                    bankAccount.EditLock,
                    Time = bankAcccountTime.ToString("o"),
                    BankTime = bankTime.ToString("o")
                };
            }
            return SerializeJson(new
            {
                Result = "success",
                Player = SelectPlayerDataForBankAccount(player),
                BankAccount = bankAccountSent
            });
        }

        private static object SelectPlayerDataForBankAccount(Player player)
        {
            return new
            {
                player.Id,
                player.Username,
                Brand = new
                {
                    Id = player.BrandId
                }
            };
        }

        public ActionResult SaveBankAccount(EditPlayerBankAccountCommand model)
        {
            try
            {
                var isExistingBankAccount = model.Id.HasValue;

                if (isExistingBankAccount)
                {
                    _playerBankAccountCommands.Edit(model);
                    return this.Success();
                }

                _playerBankAccountCommands.Add(model);
                return this.Success();
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponseActionResult(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public ActionResult SetCurrentBankAccount(Guid id)
        {
            try
            {
                _playerBankAccountCommands.SetCurrent(id);

                return this.Success();
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponseActionResult(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var vipLevels = _queries.VipLevels.ToArray();
            
            var playerPaymentLevels = _paymentQueries.GetPlayerPaymentLevels().Include(x => x.PaymentLevel).ToArray();
            
            var brands = CurrentBrands;

            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            
            var query = _queries.GetPlayers().Where(p => brands.Contains(p.BrandId) && brandFilterSelections.Contains(p.BrandId));
            
            var dataBuilder = new SearchPackageDataBuilder<Core.Player.Data.Player>(searchPackage, query);

            dataBuilder
                .SetFilterRule(x => x.BrandId, (value) => p => p.BrandId == new Guid(value))
                .Map(player => player.Id,
                    player => new[]
                    {
                        player.Username,
                        player.FirstName,
                        player.LastName,
                        string.Empty, //affilaite code
                        player.Gender.ToString(),
                        player.Email,
                        player.PhoneNumber,
                        Format.FormatDate(player.DateRegistered, true),
                        player.IpAddress,
                        (Format.FormatDate(player.DateOfBirth, false) == "0001-01-01"
                            ? null
                            : Format.FormatDate(player.DateOfBirth, false)),
                        string.Empty, //language
                        player.CurrencyCode ?? string.Empty,
                        player.CountryCode ?? string.Empty,
                        Enum.GetName(typeof (AccountStatus), player.AccountStatus),
                        _brandQueries.GetBrandOrNull(player.BrandId).Name,
                        string.Empty, //licensee
                        player.VipLevel != null && vipLevels.SingleOrDefault(x => x.Id == player.VipLevel.Id) != null
                            ? vipLevels.Single(x => x.Id == player.VipLevel.Id).Code
                            : string.Empty,
                        playerPaymentLevels.FirstOrDefault(x => x.PlayerId == player.Id) == null
                            ? string.Empty
                            : playerPaymentLevels.FirstOrDefault(x => x.PlayerId == player.Id).PaymentLevel.Name,
                        string.Empty, //fraud risk level
                        player.MailingAddressLine1,
                        player.MailingAddressCity,
                        player.MailingAddressPostalCode,
                        string.Empty, //province
                        player.VipLevel != null && vipLevels.SingleOrDefault(x => x.Id == player.VipLevel.Id) != null
                            ? vipLevels.Single(x => x.Id == player.VipLevel.Id).ColorCode
                            : string.Empty,
                    }
                );
            return dataBuilder.GetPageData(player => player.Username);
        }
    }
}