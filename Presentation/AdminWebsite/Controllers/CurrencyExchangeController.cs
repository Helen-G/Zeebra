using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class CurrencyExchangeController : BaseController
    {
        private readonly PaymentQueries _paymentQueries;
        private readonly CurrencyExchangeCommands _currencyExchangeCommandsCommands;
        private readonly UserService _userService;
        private readonly ISecurityProvider _securityProvider;

        public CurrencyExchangeController(PaymentQueries paymentQueries,
            CurrencyExchangeCommands currencyExchangeCommandsCommands,
            ISecurityProvider securityProvider,
            UserService userService)
        {
            _paymentQueries = paymentQueries;
            _currencyExchangeCommandsCommands = currencyExchangeCommandsCommands;
            _securityProvider = securityProvider;
            _userService = userService;
        }

        public string GetEditData(string id)
        {
            var brandId = new Guid(id.Split(',')[0]);
            var currencyCode = id.Split(',')[1];

            var currencyexchange = _paymentQueries.GetCurrencyExchange(brandId, currencyCode);
            var obj = new
            {
                licenseeId = currencyexchange.Brand.LicenseeId,
                licenseeName = currencyexchange.Brand.LicenseeName,
                brandId = currencyexchange.Brand.Id,
                brandName = currencyexchange.Brand.Name,
                baseCurrency = currencyexchange.Brand.BaseCurrencyCode,
                currency = currencyexchange.CurrencyTo.Code,
                currencyCode = currencyexchange.CurrencyTo.Code,
                currentRate = currencyexchange.CurrentRate,
                previousRate = currencyexchange.PreviousRate,
            };
            return SerializeJson(obj);
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            return new JsonResult { Data = SearchData(searchPackage), MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(_securityProvider.User.UserId);

            var currencyExchanges = _paymentQueries.GetCurrencyExchangesbyBrand(new Guid())
                .Where(x => brandFilterSelections.Contains(x.BrandId));

            var dataBuilder = new SearchPackageDataBuilder<CurrencyExchange>(searchPackage, currencyExchanges.AsQueryable());

            dataBuilder.Map(record => record.BrandId + "," + record.CurrencyTo.Code, record => new object[]
            {
                record.Brand.LicenseeName,
                record.Brand.Name,
                record.CurrencyTo.Code,
                record.CurrencyTo.Name,
                record.CurrentRate,
                record.IsBaseCurrency,
            });

            return dataBuilder.GetPageData(record => record.CurrencyTo.Code);
        }

        public ActionResult AddExchangeRate(SaveCurrencyExchangeData data)
        {
            try
            {
                var currencyExchange = new SaveCurrencyExchangeData
                {
                    BrandId = data.BrandId,
                    CreatedBy = "CreatedBy",
                    DateCreated = DateTimeOffset.UtcNow,
                    Currency = data.Currency,
                    CurrentRate = data.CurrentRate
                };

                _currencyExchangeCommandsCommands.Add(currencyExchange);

                return this.Success();

            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public dynamic UpdateExchangeRate(SaveCurrencyExchangeData data)
        {
            try
            {
                _currencyExchangeCommandsCommands.Save(data);
                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public dynamic RevertExchangeRate(SaveCurrencyExchangeData data)
        {
            try
            {
                _currencyExchangeCommandsCommands.Revert(data);
                return this.Success();
            }
            catch (RegoException exception)
            {
                return this.Failed(exception);
            }
        }

        public JsonResult GetLicenseeNames()
        {

            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            var licensees = _paymentQueries.GetLicensees()
                .Where(l => licenseeFilterSelections.Contains(l.Id));

            return Json(licensees.Select(l => new
            {
                Name = l.Name
            }), JsonRequestBehavior.AllowGet);
        }

        public string GetLicensees()
        {
            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);

            return SerializeJson(new
            {
                Licensees = _paymentQueries.GetLicensees()
                    .Where(l => licenseeFilterSelections.Contains(l.Id))
                .Select(c => new { Id = c.Id, Name = c.Name })
            });

        }

        public JsonResult GetBrandNames()
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(_securityProvider.User.UserId);

            var brands = _paymentQueries.GetBrands()
                .Where(b => brandFilterSelections.Contains(b.Id));

            return Json(brands.Select(b => new
            {
                Name = b.Name
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBrands()
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(_securityProvider.User.UserId);

            var brands = _paymentQueries.GetBrands()
                .Where(b => brandFilterSelections.Contains(b.Id));

            return Json(brands.Select(b => new
            {
                Id = b.Id,
                Name = b.Name
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBrandCurencies(string id)
        {
            var brandId = new Guid(id.Split(',')[0]);

            var currencies = _paymentQueries.GetBrandCurrencies(brandId);

            return Json(currencies.Select(c => new
            {
                code = c.CurrencyCode
            }), JsonRequestBehavior.AllowGet);
        }

        public string GetLicenseeBrands(Guid licenseeId, bool useBrandFilter)
        {
            var brands = _paymentQueries.GetBrands().Where(b => b.LicenseeId == licenseeId);

            if (useBrandFilter)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

                brands = brands.Where(b => brandFilterSelections.Contains(b.Id));
            }

            return SerializeJson(new
            {
                Brands = brands.OrderBy(b => b.Name).Select(b => new { b.Name, b.Id, b.BaseCurrencyCode })
            });
        }

        public string GetBrandCurenciesCode(string brandId)
        {
            var currencies = _paymentQueries.GetBrandCurrencies(new Guid(brandId));

            return SerializeJson(new
            {
                Curencies = currencies.OrderBy(c => c.CurrencyCode).Select(c => new { c.CurrencyCode })
            });
        }
    }
}
