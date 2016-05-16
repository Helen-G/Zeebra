using System.Linq;
using System.Web.Http;
using AFT.RegoV2.AdminApi.Controllers.Base;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;

namespace AFT.RegoV2.AdminApi.Controllers.Admin
{
    [Authorize]
    public class CurrencyController : BaseApiController
    {
        private readonly CurrencyCommands _currencyCommands;

        private readonly PaymentQueries _paymentQueries;
        private readonly PaymentCommands _paymentCommands;

        public CurrencyController(
            CurrencyCommands currencyCommands,
            PaymentQueries paymentQueries,
            PaymentCommands paymentCommands,
            IPermissionService permissionService)
            : base(permissionService)
        {
            _currencyCommands = currencyCommands;
            _paymentQueries = paymentQueries;
            _paymentCommands = paymentCommands;
        }

        [HttpGet]
        [Filters.SearchPackageFilter("searchPackage")]
        [Route("Currency/List")]
        public object List([FromUri] SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<Currency>(searchPackage,
                _paymentQueries.GetCurrencies().ToArray().AsQueryable());

            dataBuilder.Map(
                c => c.Code,
                c => new object[]
                {
                    c.Code,
                    c.Name,
                    c.Status.ToString(),
                    c.CreatedBy,
                    Format.FormatDate(c.DateCreated, false),
                    c.UpdatedBy,
                    Format.FormatDate(c.DateUpdated, false),
                    c.ActivatedBy,
                    Format.FormatDate(c.DateActivated, false),
                    c.DeactivatedBy,
                    Format.FormatDate(c.DateDeactivated,false),
                    c.Remarks
                });

            return dataBuilder.GetPageData(c => c.Code);
        }

        [HttpGet]
        [Route("Currency/GetByCode")]
        public object GetByCode(string code)
        {
            var currency = _paymentQueries.GetCurrency(code);

            return new
            {
                currency.Code,
                currency.Name,
                currency.Remarks,
                OldName = currency.Name
            };
        }

        [HttpPost]
        [Route("Currency/Activate")]
        public IHttpActionResult Activate(ActivateCurrencyData data)
        {
            VerifyPermission(Permissions.Activate, Modules.CurrencyManager);
            _paymentCommands.ActivateCurrency(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route("Currency/Deactivate")]
        public IHttpActionResult Deactivate(DeactivateCurrencyData data)
        {
            VerifyPermission(Permissions.Deactivate, Modules.CurrencyManager);
            _paymentCommands.DeactivateCurrency(data.Code, data.Remarks);
            return Ok(new { result = "success" });
        }

        [HttpPost]
        [Route("Currency/Save")]
        public IHttpActionResult Save(EditCurrencyData data)
        {
            var newCurrency = string.IsNullOrEmpty(data.OldCode);
            VerifyPermission(newCurrency ? Permissions.Add : Permissions.Edit, Modules.CurrencyManager);

            var result = newCurrency
                    ? _currencyCommands.Add(data)
                    : _currencyCommands.Save(data);

            return Ok(new { Result = "success", Data = result });
        }
    }
}
