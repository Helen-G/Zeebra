using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Infrastructure.Sms;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    public class AvcConfigurationTests : AdminWebsiteUnitTestsBase
    {
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private IAVCConfigurationQueries _avcConfigurationQueries;
        private BrandQueries _brandQueries;
        private FakeBrandRepository _fakeBrandRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeBrandRepository = Container.Resolve<FakeBrandRepository>();

            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _avcConfigurationQueries = Container.Resolve<IAVCConfigurationQueries>();
            _brandQueries = Container.Resolve<BrandQueries>();

            Container.Resolve<SecurityTestHelper>().SignInUser();

            Container.Resolve<RiskLevelWorker>().Start();
        }

        [Test]
        public void Can_create_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false,
                HasWinnings = true,
                WinningRules = new List<WinningRuleDTO>
                {
                    FraudTestDataHelper.GenerateWinningRule(),
                    FraudTestDataHelper.GenerateWinningRule()
                }
            };

            _avcConfigurationCommands.Create(avcConfiguration);
            Assert.NotNull(_avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id));
        }

        [Test]
        public void Can_delete_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO()
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false,
                HasWinnings = true,
                WinningRules = new List<WinningRuleDTO>
                {
                    FraudTestDataHelper.GenerateWinningRule(),
                    FraudTestDataHelper.GenerateWinningRule()
                }
            };
            _avcConfigurationCommands.Create(avcConfiguration);
            _avcConfigurationCommands.Delete(id);

            Assert.IsEmpty(_avcConfigurationQueries.GetAutoVerificationCheckConfigurations());
        }

        [Test]
        public void Can_update_Avc_configuration()
        {
            var id = Guid.NewGuid();
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var avcConfiguration = new AVCConfigurationDTO()
            {
                Id = id,
                Brand = _brandQueries.GetBrands().First().Id,
                Currency = _brandQueries.GetBrands().First().DefaultCurrency,
                HasFraudRiskLevel = false
            };
            _avcConfigurationCommands.Create(avcConfiguration);

            avcConfiguration.HasFraudRiskLevel = true;
            avcConfiguration.HasWinnings = true;

            avcConfiguration.WinningRules = new List<WinningRuleDTO>
            {
                FraudTestDataHelper.GenerateWinningRule()
            };

            _avcConfigurationCommands.Update(avcConfiguration);
            Assert.IsTrue(_avcConfigurationQueries.GetAutoVerificationCheckConfiguration(id).HasFraudRiskLevel);
        }
    }
}