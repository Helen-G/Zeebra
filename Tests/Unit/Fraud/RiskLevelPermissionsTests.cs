using System;
using AFT.RegoV2.ApplicationServices.Security;
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
    class RiskLevelPermissionsTests : PermissionsTestsBase
    {
        private IRiskLevelQueries _riskQueries;
        private IRiskLevelCommands _riskCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _riskQueries = Container.Resolve<IRiskLevelQueries>();
            _riskCommands = Container.Resolve<IRiskLevelCommands>();
            Container.Resolve<RiskLevelWorker>().Start();
        }

        [Test]
        public void Cannot_execute_RiskQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.FraudManager, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskQueries.GetAll());
        }

        [Test]
        public void Cannot_execute_RiskCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.FraudManager, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Activate(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Deactivate(new Guid(), "Some remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Update(new RiskLevel()));
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Create(new RiskLevel()));
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Tag(new Guid(), new Guid(), "Some description"));
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Untag(new Guid(), "Some description"));
        }

        [Test]
        public void Cannot_activate_risk_level_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidRiskLevelData();
            _riskCommands.Create(data);
            LogWithNewUser(Modules.FraudManager, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Activate(data.Id, "Some remark"));
        }

        [Test]
        public void Cannot_deactivate_risk_level_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidRiskLevelData();
            _riskCommands.Create(data);
            LogWithNewUser(Modules.FraudManager, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Deactivate(data.Id, "Some remark"));
        }

        [Test]
        public void Cannot_update_risk_level_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidRiskLevelData();
            _riskCommands.Create(data);
            LogWithNewUser(Modules.FraudManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Update(data));
        }

        [Test]
        public void Cannot_create_risk_level_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidRiskLevelData();
            LogWithNewUser(Modules.FraudManager, Permissions.Add);

            // Act
            
        }

        [Test]
        public void Cannot_tag_player_with_invalid_brand()
        {
            // Arrange
            var playerId = CreatePlayer();
            var riskLevelData = CreateValidRiskLevelData();
            _riskCommands.Create(riskLevelData);
            LogWithNewUser(Modules.FraudManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Tag(playerId, riskLevelData.Id, "Some description"));
        }

        [Test]
        public void Cannot_untag_player_with_invalid_brand()
        {
            // Arrange
            var playerId = CreatePlayer();
            LogWithNewUser(Modules.FraudManager, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _riskCommands.Untag(playerId, "Some description"));
        }

        private RiskLevel CreateValidRiskLevelData()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var data = new RiskLevel
            {
                Id = new Guid(),
                BrandId = brand.Id,
                Name = "risk_level_test",
                Level = 1001,
                Description = "remarks"
            };

            return data;
        }

        private Guid CreatePlayer()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var playerTestHelper = Container.Resolve<PlayerTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var player = playerTestHelper.CreatePlayer(true, brand.Id);

            return player.Id;
        }
    }
}
