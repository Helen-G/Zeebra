using System;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public class WageringConfigurationTests : AdminWebsiteUnitTestsBase
    {
        private IWagerConfigurationCommands _wagerConfigurationCommands;
        private IWagerConfigurationQueries _wagerConfigurationQueries;
        private FakeBrandRepository _brandRepository;
        private BrandTestHelper _brandTestHelper;
        private SecurityTestHelper _securityTestHelper;
        private Core.Brand.Data.Brand _brand;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _wagerConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();
            _wagerConfigurationQueries = Container.Resolve<IWagerConfigurationQueries>();
            _brandRepository = Container.Resolve<FakeBrandRepository>();
            _brandTestHelper = Container.Resolve<BrandTestHelper>();
            _securityTestHelper = Container.Resolve<SecurityTestHelper>();

            _securityTestHelper.SignInUser();
            
            foreach (var currencyCode in TestDataGenerator.CurrencyCodes)
            {
                _brandRepository.Currencies.Add(new Currency { Code = currencyCode });
            }

            _securityTestHelper.SignInUser();
            _brand = _brandTestHelper.CreateBrand(isActive: true);
        }

        [Test]
        public void Can_create_wagering_configuration()
        {
            var wagerId =_wagerConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO
            {
                BrandId = _brand.Id,
                IsDepositWageringCheck = true,
                IsManualAdjustmentWageringCheck = true,
                IsRebateWageringCheck = true,
                Currency = _brand.DefaultCurrency
            }, Guid.Empty);

            var wageringConfiguration = _wagerConfigurationQueries.GetWagerConfiguration(wagerId);

            Assert.IsNotNull(wageringConfiguration);
        }

        [Test]
        public void Cannnot_create_wagering_configuration_without_criteria()
        {
            var wageringConfigDTO = new WagerConfigurationDTO
            {
                BrandId = _brand.Id,
                IsDepositWageringCheck = false,
                IsManualAdjustmentWageringCheck = false,
                IsRebateWageringCheck = false,
                Currency = _brand.DefaultCurrency
            };

            Assert.Throws<RegoValidationException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(wageringConfigDTO, Guid.Empty));

        }

        [Test]
        public void Cannnot_create_wagering_configuration_without_currency()
        {
            var wageringConfigDTO = new WagerConfigurationDTO
            {
                BrandId = _brand.Id,
                IsDepositWageringCheck = false,
                IsManualAdjustmentWageringCheck = true,
                IsRebateWageringCheck = false
            };

            Assert.Throws<RegoValidationException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(wageringConfigDTO, Guid.Empty));

        }

        [Test]
        public void Cannnot_create_duplicate_wagering_configuration()
        {
            var wageringConfigDTOFirst = new WagerConfigurationDTO
            {
                BrandId = _brand.Id,
                IsDepositWageringCheck = false,
                IsManualAdjustmentWageringCheck = true,
                IsRebateWageringCheck = false,
                Currency = _brand.DefaultCurrency
            };
            
            var wageringConfigDTOSecond = new WagerConfigurationDTO
            {
                BrandId = _brand.Id,
                IsDepositWageringCheck = false,
                IsManualAdjustmentWageringCheck = true,
                IsRebateWageringCheck = false,
                Currency = _brand.DefaultCurrency
            };

            _wagerConfigurationCommands.CreateWagerConfiguration(wageringConfigDTOFirst, Guid.Empty);

            Assert.Throws<RegoValidationException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(wageringConfigDTOSecond, Guid.Empty));

        }
    }
}