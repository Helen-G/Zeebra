using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using AFT.RegoV2.Infrastructure.Constants;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;

namespace AFT.RegoV2.Tests.Unit.Game
{
    [Ignore("Tsonko should fix tests in this class")]
    internal class GameManagerPermissionTests : PermissionsTestBase
    {
        private IGameCommands _commands;
        private IGameQueries _queries;
        private IGameRepository _repository;
        private GamesTestHelper _gsiHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _queries = Container.Resolve<IGameQueries>();
             _commands = Container.Resolve<IGameCommands>();
             _repository = Container.Resolve<IGameRepository>();
             _gsiHelper = Container.Resolve<GamesTestHelper>();

            _gsiHelper.CreateGameServer();
        }

        [Test]
        public void Cannot_create_game_without_permissions()
        {
            /*** Arrange ***/

            // Create role and user that has permission to create new game
            var userWithoutPermission = CreateUserWithPermissions(Core.Security.Common.Modules.GameManager, new string[0]);

            var gameDto = CreateGameDto();

            /*** Act ***/
            UserService.SignInUser(userWithoutPermission);
            // Check that method CreateGame throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _commands.CreateGame(gameDto, userWithoutPermission.Username));
        }

        [Test]
        public void Cannot_update_game_without_permissions()
        {
            /*** Arrange ***/
            var gameDto = CreateGameDto();

            var userWithoutPermission = CreateUserWithPermissions(Core.Security.Common.Modules.GameManager, new string[0]);
            var userWithCreatePermission = CreateUserWithPermissions(Core.Security.Common.Modules.GameManager, new[] { Permissions.Add });

            UserService.SignInUser(userWithCreatePermission);
            _commands.CreateGame(gameDto, userWithCreatePermission.Username);

            Assert.AreEqual(2, _queries.GetGameEndpoints().Count());

            var game = _queries.GetGameEndpoints().First();

            /*** Act ***/
            UserService.SignInUser(userWithoutPermission);
            game.Name = "Game updated";
            // Check that method UpdateGame throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _commands.UpdateGame(game, userWithoutPermission.Username));
        }

        private GameDTO CreateGameDto()
        {
            var gameDto = new GameDTO()
            {
                Name = "Game",
                Type = "Casino",
                Status = "Active",
                Url = "http://localhost/",
                ProductId = _repository.GameServers.First().Id
            };
            return gameDto;
        }

        [Test]
        public void Cannot_delete_Game_without_permissions()
        {
            /*** Arrange ***/
            var gameDto = CreateGameDto();

            var userWithoutPermission = CreateUserWithPermissions(Core.Security.Common.Modules.GameManager, new[] { Permissions.Add });
            var userWithPermission = CreateUserWithPermissions(Core.Security.Common.Modules.GameManager, new[] { Permissions.Add });

            UserService.SignInUser(userWithPermission);

            _commands.CreateGame(gameDto, userWithPermission.Username);

            Assert.AreEqual(2, _queries.GetGameEndpoints().Count());

            var game = _queries.GetGameEndpoints().First();

            /*** Act ***/
            UserService.SignInUser(userWithoutPermission);
            // Check that method DeleteGame throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _commands.DeleteGame((Guid)game.Id));
        }
    }
}