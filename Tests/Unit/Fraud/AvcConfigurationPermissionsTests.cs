using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    class AvcConfigurationPermissionsTests : PermissionsTestsBase
    {
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private IAVCConfigurationQueries _avcConfigurationQueries;
        private BrandQueries _brandQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _avcConfigurationQueries = Container.Resolve<IAVCConfigurationQueries>();
            _brandQueries = Container.Resolve<BrandQueries>();
            Container.Resolve<RiskLevelWorker>().Start();
        }

        [Test]
        public void Cannot_execute_AVCConfigurationQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.AutoVerificationConfiguration, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _avcConfigurationQueries.GetAutoVerificationCheckConfigurations());
        }

        [Test]
        public void Cannot_execute_AVCConfigurationCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.AutoVerificationConfiguration, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _avcConfigurationCommands.Create(new AVCConfigurationDTO()));
        }

        [Test]
        public void Cannot_create_avc_configuration_with_invalid_brand()
        {
            // Arrange
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

            LogWithNewUser(Modules.AutoVerificationConfiguration, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _avcConfigurationCommands.Create(avcConfiguration));
        }
    }
}
