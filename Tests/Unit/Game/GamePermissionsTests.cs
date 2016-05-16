using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Game
{
    class GamePermissionsTests : PermissionsTestsBase
    {
        private IGameManagement _gameManagement;
        private IGameQueries _gameQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _gameManagement = Container.Resolve<IGameManagement>();
            _gameQueries = Container.Resolve<IGameQueries>();
        }

        [Test]
        public void Cannot_execute_GameQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.FraudManager, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _gameQueries.GetGameDtos());
            Assert.Throws<InsufficientPermissionsException>(() => _gameQueries.GetGameProviderDtos());
            Assert.Throws<InsufficientPermissionsException>(() => _gameQueries.GetBrandProducts());
        }

        [Test]
        public void Cannot_execute_GameCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.GameManager, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.CreateGame(new GameDTO()));
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.UpdateGame(new GameDTO()));
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.DeleteGame(new Guid()));
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.UpdateProductSettings(new BrandProductSettingsData()));
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.CreateGameProvider(new GameProvider()));
            Assert.Throws<InsufficientPermissionsException>(() => _gameManagement.UpdateGameProvider(new GameProvider()));
        }
    }
}