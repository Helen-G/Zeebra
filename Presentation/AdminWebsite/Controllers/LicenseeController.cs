using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AutoMapper;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class LicenseeController : BaseController
    {
        private readonly BrandQueries _queries;
        private readonly GameRepository _gameRepository;
        private readonly IGameQueries _gameQueries;
        private readonly LicenseeCommands _licenseeCommands;
        private readonly LicenseeQueries _licenseeQueries;
        private readonly UserService _userService;

        public LicenseeController(            
            BrandQueries queries,
            GameRepository gameRepository,
            IGameQueries gameQueries, 
            LicenseeCommands licenseeCommands, 
            LicenseeQueries licenseeQueries, 
            UserService userService)
        {
            _queries = queries;
            _gameRepository = gameRepository;
            _gameQueries = gameQueries;
            _licenseeCommands = licenseeCommands;
            _licenseeQueries = licenseeQueries;
            _userService = userService;
        }

        static LicenseeController()
        {
            Mapper.CreateMap<EditLicenseeModel, AddLicenseeData>()
                .ForMember(dest => dest.ContractStart, opt => opt.ResolveUsing(src => Format.FormatDateString(src.ContractStart)))
                .ForMember(dest => dest.ContractEnd, opt => opt.ResolveUsing(src => Format.FormatDateString(src.ContractEnd)));

            Mapper.CreateMap<EditLicenseeModel, EditLicenseeData>()
                .ForMember(dest => dest.ContractStart, opt => opt.ResolveUsing(src => Format.FormatDateString(src.ContractStart)))
                .ForMember(dest => dest.ContractEnd, opt => opt.ResolveUsing(src => Format.FormatDateString(src.ContractEnd)));
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);
            var licensees = _queries.GetAllLicensees().Where(x => licenseeFilterSelections.Contains(x.Id));
            var dataBuilder = new SearchPackageDataBuilder<Licensee>(searchPackage, licensees);

            dataBuilder.Map(license => license.Id, license => new object[]
            {
                license.Name,
                license.CompanyName,
                Format.FormatDate(license.ContractStart, false),
                license.ContractEnd == null ? "Open Ended" : Format.FormatDate(license.ContractEnd, false),
                Enum.GetName(typeof (LicenseeStatus), license.Status),
                license.CreatedBy,
                Format.FormatDate(license.DateCreated, false),
                license.UpdatedBy,
                Format.FormatDate(license.DateUpdated, false),
                license.ActivatedBy,
                Format.FormatDate(license.DateActivated, false),
                license.DeactivatedBy,
                Format.FormatDate(license.DateDeactivated, false),
                _queries.CanActivateLicensee(license),
                license.Remarks,
                _queries.CanRenewLicenseeContract(license)
            });

            var data = dataBuilder.GetPageData(license => license.Name);

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult Save(EditLicenseeModel model)
        {
            try
            {
                var id = model.Id;                

                if (id.HasValue)
                {
                    var editData = Mapper.DynamicMap<EditLicenseeData>(model);
                    _licenseeCommands.Edit(editData);                 
                }
                else
                {
                    var addData = Mapper.DynamicMap<AddLicenseeData>(model);
                    id = _licenseeCommands.Add(addData);
                }

                return this.Success(new {id});
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

        public string GetForEdit(string id)
        {
            var licensees = _queries.GetFilteredLicensees(_queries.GetLicensees(), CurrentUser.UserId);

            var guid = new Guid(id);

            var licensee = licensees.SingleOrDefault(x => x.Id == guid);

            return licensee == null
                ? SerializeJson(new {Result = "failed"})
                : SerializeJson(new
                {
                    Result = "success",
                    Data =
                        new
                        {
                            licensee.Name,
                            licensee.CompanyName,
                            licensee.AffiliateSystem,
                            ContractStart = licensee.ContractStart.ToString("o"),
                            ContractEnd = licensee.ContractEnd.HasValue
                                ? licensee.ContractEnd.Value.ToString("o")
                                : null,
                            licensee.Email,
                            licensee.AllowedBrandCount,
                            licensee.AllowedWebsiteCount,
                            licensee.TimezoneId,
                            GameProviders = licensee.Products.Select(x => x.ProductId),
                            licensee.Currencies,
                            licensee.Countries,
                            licensee.Cultures,
                            Status = Enum.GetName(typeof (LicenseeStatus), licensee.Status)
                        }
                });
        }

        public object GetViewData(Guid id)
        {
            var licensee = _queries.GetLicensee(id);

            if (licensee == null)
                return SerializeJson(new {result = "failed"});

            var data = new
            {
                name = licensee.Name,
                companyName = licensee.CompanyName,
                affiliateSystem = licensee.AffiliateSystem,
                contractStartDate = Format.FormatDate(licensee.ContractStart, false),
                contractEndDate = licensee.ContractEnd.HasValue ? Format.FormatDate(licensee.ContractEnd, false) : null,
                email = licensee.Email,
                timezone = TimeZoneInfo.FindSystemTimeZoneById(licensee.TimezoneId).DisplayName,
                status = Enum.GetName(typeof (LicenseeStatus), licensee.Status),
                allowedBrands = licensee.AllowedBrandCount,
                allowedWebsites = licensee.AllowedWebsiteCount,
                currencies = licensee.Currencies.OrderBy(x => x.Code).Select(x => x.Code),
                countries = licensee.Countries.OrderBy(x => x.Name).Select(x => x.Name),
                cultures = licensee.Cultures.OrderBy(x => x.Code).Select(x => x.Code),
                products = _gameQueries.GetGameProviderDtos()
                    .Where(x => licensee.Products.Any(y => y.ProductId == x.Id))
                    .Select(x => x.Name)
            };

            return SerializeJson(new {result = "success", data});
        }

        public string GetLicensees()
        {
            var filteredLicensees = _queries.GetFilteredLicensees(_queries.GetLicensees(), CurrentUser.UserId);
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            var licensees = filteredLicensees
                .OrderBy(l => l.Name)
                .Select(l => new
                {
                    l.Name, 
                    l.Id,
                    IsSelectedInFilter = licenseeFilterSelections.Contains(l.Id)
                });

            return SerializeJson(new {licensees});
        }

        public ActionResult GetLicensee(string id)
        {
            var licensees = _queries.GetFilteredLicensees(_queries.GetLicensees(), CurrentUser.UserId);
            var guid = Guid.Parse(id);
            var licensee = licensees.SingleOrDefault(x => x.Id == guid);
            if (licensee == null)
                return null;
            return Json(new {Name = licensee.Name, Id = licensee.Id}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBrand(string id)
        {
            var brands = _queries.GetFilteredBrands(_queries.GetBrands(), CurrentUser.UserId);
            var guid = Guid.Parse(id);
            var brand = brands.SingleOrDefault(x => x.Id == guid);
            if (brand == null)
                return null;
            return Json(new {Name = brand.Name, Id = brand.Id}, JsonRequestBehavior.AllowGet);
        }

        public string GetBrands(Guid licensee)
        {
            var brands = _queries.GetFilteredBrands(_queries.GetBrandsByLicensee(licensee), CurrentUser.UserId);

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(b => b.Name)
                    .Select(b => new {name = b.Name, id = b.Id})
            });
        }

        [HttpGet]
        public ActionResult GetBrandsByLicensee(Guid licensee)
        {
            var response = new
            {
                Brands = _queries.GetBrandsByLicensee(licensee)
                    .Select(b => new {name = b.Name, id = b.Id})
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetInactiveBrandsByLicensee(Guid licensee)
        {
            var inactiveBrands = _queries.GetBrandsByLicensee(licensee)
                .Where(x => x.Status == BrandStatus.Inactive);

            var response = new
            {
                Brands = inactiveBrands.Select(b => new {name = b.Name, id = b.Id})
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetEditData()
        {
            try
            {
                return this.Success(new
                {
                    GameProviders = _gameRepository.GameProviders,
                                        
                    Currencies = _queries.GetCurrencies(),
                    Countries = _queries.GetCountries(),
                    languages = _queries.GetActiveCultures(),
                    timeZones = TimeZoneInfo.GetSystemTimeZones().Select(x => new {x.Id, x.DisplayName})
                });
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpGet]
        [SearchPackageFilter("searchPackage")]
        public ActionResult Contracts(SearchPackage searchPackage, Guid licenseeId)
        {
            var contracts = _queries.GetLicenseeContracts(licenseeId).AsQueryable();

            var dataBuilder = new SearchPackageDataBuilder<Contract>(searchPackage, contracts);

            dataBuilder.Map(contract => contract.Id, contract => new object[]
            {
                Format.FormatDate(contract.StartDate, false),
                contract.EndDate.HasValue ? Format.FormatDate(contract.EndDate, false) : null,
                _licenseeQueries.GetContractStatus(contract)
            });

            var data = dataBuilder.GetPageData(contract => contract.IsCurrentContract);

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult Activate(Guid id, string remarks)
        {
            try
            {
                _licenseeCommands.Activate(id, remarks);
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

        [HttpPost]
        public ActionResult Deactivate(Guid id, string remarks)
        {
            try
            {
                _licenseeCommands.Deactivate(id, remarks);
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

        [HttpGet]
        public string RenewContract(Guid licenseeId)
        {
            var licensee = _licenseeQueries.GetRenewContractData(licenseeId);

            if (licensee == null) return null;

            var data = new
            {
                name = licensee.Name,
                companyName = licensee.CompanyName,
                affiliateSystem = licensee.AffiliateSystem,
                contractStart = Format.FormatDate(licensee.ContractStart, false),
                contractEnd = licensee.ContractEnd.HasValue ? Format.FormatDate(licensee.ContractEnd, false) : null,
                email = licensee.Email,
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(licensee.TimezoneId).DisplayName,
                contractStatus = _licenseeQueries.GetContractStatus(licensee.Contracts.Single(x => x.IsCurrentContract))
            };

            return SerializeJson(data);
        }

        [HttpPost]
        public ActionResult RenewContract(Guid licenseeId, string contractStart, string contractEnd)
        {
            try
            {
                _licenseeCommands.RenewContract(licenseeId, contractStart, contractEnd);
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

        [HttpGet]
        public string Licensees(bool useFilter)
        {
            var licensees = _queries.GetFilteredLicensees(_queries.GetLicensees(), CurrentUser.UserId);

            if (useFilter)
            {
                var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

                licensees = licensees.Where(x => licenseeFilterSelections.Contains(x.Id));
            }

            return SerializeJson(new
            {
                licensees = licensees.OrderBy(x => x.Name).Select(x => new { x.Id, x.Name })
            });
        }
    }
}