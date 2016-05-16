using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using FluentValidation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AutoVerificationController : BaseController
    {
        private readonly IAVCConfigurationCommands _avcConfigurationCommands;
        private readonly IAVCConfigurationQueries _avcConfigurationQueries;
        private readonly IRiskLevelQueries _riskLevelQueries;
        private readonly BrandQueries _brandQueries;
        private readonly ISecurityRepository _securityRepository;
        private readonly IGameQueries _gameQueries;
        private readonly PaymentLevelQueries _paymentQueries;

        public AutoVerificationController(
            IAVCConfigurationCommands avcConfigurationCommands,
            IAVCConfigurationQueries avcConfigurationQueries,
            IRiskLevelQueries riskLevelQueries,
            BrandQueries brandQueries, 
            IGameQueries gameQueries,
            ISecurityRepository securityRepository,
            PaymentLevelQueries paymentQueries)
        {
            _avcConfigurationCommands = avcConfigurationCommands;
            _avcConfigurationQueries = avcConfigurationQueries;
            _riskLevelQueries = riskLevelQueries;
            _brandQueries = brandQueries;
            _gameQueries = gameQueries;
            _securityRepository = securityRepository;
            _paymentQueries = paymentQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<AutoVerificationCheckConfiguration>(
                searchPackage,
                _avcConfigurationQueries.GetAutoVerificationCheckConfigurations().AsQueryable());
            dataBuilder.SetFilterRule(x => x.Brand, value => p => p.Brand.Id == Guid.Parse(value))
                .Map(configuration => configuration.Id,
                    obj => new[]
                    {
                        obj.Brand.LicenseeName,
                        obj.Brand.Name,
                        obj.Currency,
                        _brandQueries.GetVipLevel(obj.VipLevelId).Name,
                        GetCriteriasString(obj),
                        Format.FormatDate(obj.DateCreated, false),
                        _securityRepository.Users.Single(x => x.Id == obj.CreatedBy).Username
                    });
            var data = dataBuilder.GetPageData(configuration => configuration.Brand.Name);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        private string GetCriteriasString(AutoVerificationCheckConfiguration autoVerificationCheckConfiguration)
        {
            var type = autoVerificationCheckConfiguration.GetType();
            var criteriasCount =
                    type
                    .GetProperties()
                    .Where(x => x.Name.StartsWith("has", StringComparison.InvariantCultureIgnoreCase))
                    .Count(x => (bool)x.GetValue(autoVerificationCheckConfiguration));
            
            return string.Format("Criterias count: {0}", criteriasCount);
        }

        [HttpPost]
        public ActionResult Verification(AVCConfigurationDTO data)
        {
            try
            {
                if (data.Id == Guid.Empty)
                    _avcConfigurationCommands.Create(data);
                else
                    _avcConfigurationCommands.Update(data);
            }
            catch (ValidationException e)
            {
                return this.Failed(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
            return this.Success();
        }

        public string GetById(Guid id)
        {
            var configuration = _avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id);
            return SerializeJson(configuration);
        }

        public object GetCurrencies(Guid brandId)
        {
            return SerializeJson(new
            {
                Currencies = _brandQueries.GetCurrenciesByBrand(brandId).OrderBy(x => x.Code).Select(x => x.Code ),
            });
        }

        public string GetFraudRiskLevels(Guid brandId)
        {
            var riskLevels = _riskLevelQueries.GetByBrand(brandId).ToList();
            return SerializeJson(new
            {
                RiskLevels = riskLevels.Select(x => new {id = x.Id, name = x.Name})
            });
        }

        /// <summary>
        /// The method returns all the payment levels that have brandId and currencyCode as the passed ones
        /// </summary>
        /// <param name="brandId"></param>
        /// <param name="currencyCode"></param>
        /// <returns>Pairs of payment level ID and payment level Name</returns>
        public string GetPaymentLevels(Guid brandId, string currencyCode)
        {
            var paymentLevels = _paymentQueries.GetPaymentLevelsByBrandAndCurrency(brandId, currencyCode).ToList();
                               
            return SerializeJson(new
            {
                PaymentLevels = paymentLevels.Select(x => new { id = x.Id, name = x.Name })
            });
        }

        public JsonResult GetAllowedBrandProducts(Guid brandId)
        {
            var brand = _brandQueries.GetBrand(brandId);
            var productIds = brand.Products.Select(x => x.ProductId);
            var products = ProductViewModel.BuildFromIds(_gameQueries, productIds);
            return Json(products.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}
