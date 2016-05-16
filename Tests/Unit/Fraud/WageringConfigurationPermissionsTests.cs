using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Fraud
{
    class WageringConfigurationPermissionsTests : PermissionsTestsBase
    {
        private IWagerConfigurationCommands _wagerConfigurationCommands;
        private IWagerConfigurationQueries _wagerConfigurationQueries;
        private FakeBrandRepository _brandRepository;
        private ISecurityProvider _securityProvider;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _wagerConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();
            _wagerConfigurationQueries = Container.Resolve<IWagerConfigurationQueries>();
            _brandRepository = Container.Resolve<FakeBrandRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();

            foreach (var currencyCode in TestDataGenerator.CurrencyCodes)
            {
                _brandRepository.Currencies.Add(new Currency { Code = currencyCode });
            }
        }

        [Test]
        public void Cannot_execute_WagerConfigurationQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.WagerConfiguration, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationQueries.GetWagerConfigurations());
        }

        [Test]
        public void Cannot_execute_WagerConfigurationCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.WagerConfiguration, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.ActivateWagerConfiguration(new Guid(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.DeactivateWagerConfiguration(new Guid(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO(), new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.UpdateWagerConfiguration(new WagerConfigurationDTO(), new Guid()));
        }

        [Test]
        public void Cannot_activate_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var wagerId = _wagerConfigurationCommands.CreateWagerConfiguration(CreateValidWagerConfigurationDto(), _securityProvider.User.UserId);
            LogWithNewUser(Modules.WagerConfiguration, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.ActivateWagerConfiguration(wagerId, _securityProvider.User.UserId));
        }

        [Test]
        public void Cannot_deactivate_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var wagerId = _wagerConfigurationCommands.CreateWagerConfiguration(CreateValidWagerConfigurationDto(), _securityProvider.User.UserId);
            LogWithNewUser(Modules.WagerConfiguration, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.DeactivateWagerConfiguration(wagerId, _securityProvider.User.UserId));
        }

        [Test]
        public void Cannot_create_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidWagerConfigurationDto();
            LogWithNewUser(Modules.WagerConfiguration, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.CreateWagerConfiguration(data, _securityProvider.User.UserId));
        }

        [Test]
        public void Cannot_update_wager_configuration_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidWagerConfigurationDto();
            LogWithNewUser(Modules.WagerConfiguration, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _wagerConfigurationCommands.UpdateWagerConfiguration(data, new Guid()));
        }

        private WagerConfigurationDTO CreateValidWagerConfigurationDto()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var data = new WagerConfigurationDTO()
            {
                BrandId = brand.Id,
                IsDepositWageringCheck = true,
                Currency = brand.DefaultCurrency
            };

            return data;
        }
    }
}
