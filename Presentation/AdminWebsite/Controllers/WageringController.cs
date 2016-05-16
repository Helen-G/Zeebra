using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Providers;
using ServiceStack.Common;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class WageringController : BaseController
    {
        private readonly IWagerConfigurationQueries _wagerConfigurationQueries;
        private readonly IWagerConfigurationCommands _wagerConfigurationCommands;
        private readonly BrandQueries _brandQueries;
        private readonly UserService _userService;

        public WageringController(
            IWagerConfigurationQueries wagerConfigurationQueries,
            BrandQueries brandQueries,
            IWagerConfigurationCommands wagerConfigurationCommands, 
            UserService userService)
        {
            _wagerConfigurationQueries = wagerConfigurationQueries;
            _wagerConfigurationCommands = wagerConfigurationCommands;
            _userService = userService;
            _brandQueries = brandQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult List(SearchPackage searchPackage)
        {
            var data = SearchWageringConfigurations(searchPackage);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private object SearchWageringConfigurations(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var wagerConfigurations = _wagerConfigurationQueries.GetWagerConfigurations()
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<WagerConfigurationDTO>(searchPackage, wagerConfigurations);

            dataBuilder
                .Map(wager => wager.Id.ToString(),
                    wager => new[]
                    {
                        _brandQueries.GetBrandOrNull(wager.BrandId).Licensee.Name,
                        _brandQueries.GetBrandOrNull(wager.BrandId).Name,
                        wager.Currency,
                        wager.Status,
                        wager.IsActive,
                        wager.ActivatedBy.ToString(),
                        wager.DateActivated == null ? null : Format.FormatDate(wager.DateActivated, false),
                        wager.DeactivatedBy.ToString(),
                        wager.DateDeactivated == null ? null : Format.FormatDate(wager.DateDeactivated, false),
                        wager.CreatedBy.ToString(),
                        Format.FormatDate(wager.DateCreated, false),
                        wager.UpdatedBy.ToString(),
                        wager.DateUpdated == null ? null : Format.FormatDate(wager.DateUpdated, false)
                    }
                );

            return dataBuilder.GetPageData(walletTemplate => walletTemplate.DateCreated);
        }

        [HttpPost]
        public ActionResult WagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO)
        {
            try
            {
                var message = String.Empty;
                if (wagerConfigurationDTO.Id == Guid.Empty)
                {
                    _wagerConfigurationCommands.CreateWagerConfiguration(wagerConfigurationDTO, CurrentUser.UserId);
                    message = "Wagering configuration has been created successfully";
                }
                else
                {
                    _wagerConfigurationCommands.UpdateWagerConfiguration(wagerConfigurationDTO, CurrentUser.UserId);
                    message = "Wagering configuration has been updated successfully";
                }
                return this.Success(message); 
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch(Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Activate(Guid id)
        {
            try
            {
                _wagerConfigurationCommands.ActivateWagerConfiguration(id, CurrentUser.UserId);
                return this.Success("ok");
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        [HttpPost]
        public ActionResult Deactivate(Guid id)
        {
            try
            {
                _wagerConfigurationCommands.DeactivateWagerConfiguration(id, CurrentUser.UserId);
                return this.Success("ok");
            }
            catch (ValidationError e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        public ActionResult Currencies(Guid brandId, bool isEdit)
        {
            var returnList = new List<string>();
            var currencies = _brandQueries.GetBrandOrNull(brandId).BrandCurrencies.Select(x => x.CurrencyCode);

            var existingCurrencies = _wagerConfigurationQueries
                .GetWagerConfigurations()
                .Where(x => x.BrandId == brandId)
                .Select(x => x.Currency);

            if (!isEdit)
                currencies = currencies.Except(existingCurrencies);

            returnList = currencies.ToList();
            if (existingCurrencies.Any() && (!existingCurrencies.Any(x => x.Equals("all", StringComparison.InvariantCultureIgnoreCase)) || isEdit))
                returnList.Add("All");

            return Json(returnList,JsonRequestBehavior.AllowGet);
        }

        public string Brands(Guid licensee)
        {
            var brands = _brandQueries
                .GetFilteredBrands(_brandQueries.GetBrandsByLicensee(licensee), CurrentUser.UserId)
                .Where(b => b.Status == BrandStatus.Active)
                .ToList();

            return SerializeJson(new
            {
                Brands = brands
                    .OrderBy(b => b.Name)
                    .Select(b => new { name = b.Name, id = b.Id })
            });
        }

        public ActionResult GetConfiguration(Guid id)
        {
            var configuration = _wagerConfigurationQueries.GetWagerConfiguration(id);
            return Json(configuration, JsonRequestBehavior.AllowGet);
        }
    }
}