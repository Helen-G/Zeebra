using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Game
{
    internal class GameManager : AdminWebsiteUnitTestsBase
    {
        private IGameManagement _gameManagement;
        private IGameQueries _queries;
        private IGameRepository _repository;

        public override void BeforeEach()
        {
            base.BeforeEach();
             _queries = Container.Resolve<IGameQueries>();
             _gameManagement = Container.Resolve<IGameManagement>();
            _repository = Container.Resolve<FakeGameRepository>();

            _repository.GameProviders.Add(
                new GameProvider
                {
                    Id = Guid.NewGuid(),
                    Name = "Cowsino",
                });

            var securityTestHelper = Container.Resolve<SecurityTestHelper>();
            securityTestHelper.SignInUser();
            securityTestHelper.CurrentUser.UserName = "Admin";
        }

        [Test]
        public void Can_Create_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO();
            gameDto.Name = "Game";
            gameDto.Type = "Casino";
            gameDto.Status = "Active";
            gameDto.Url = "http://localhost/";
            gameDto.ProductId = _repository.GameProviders.First().Id;

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());
        }


        [Test]
        public void Can_Edit_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO();
            gameDto.Name = "Game";
            gameDto.Type = "Casino";
            gameDto.Status = "Active";
            gameDto.Url = "http://localhost/";
            gameDto.ProductId = _repository.GameProviders.First().Id;

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());

            var game = _queries.GetGameDtos().First();

            game.Name = "Game updated";

            _gameManagement.UpdateGame(game);

            game = _queries.GetGameDtos().First();

            Assert.AreEqual("Game updated", game.Name);
            Assert.NotNull(game.UpdatedBy);
            Assert.NotNull(game.UpdatedDate);
        }

        [Test]
        public void Can_Delete_Game()
        {
            Assert.AreEqual(0, _queries.GetGameDtos().Count());

            var gameDto = new GameDTO();
            gameDto.Name = "Game";
            gameDto.Type = "Casino";
            gameDto.Status = "Active";
            gameDto.Url = "http://localhost/";
            gameDto.ProductId = _repository.GameProviders.First().Id;

            _gameManagement.CreateGame(gameDto);

            Assert.AreEqual(1, _queries.GetGameDtos().Count());

            var game = _queries.GetGameDtos().First();

            _gameManagement.DeleteGame((Guid)game.Id);

            Assert.AreEqual(0, _queries.GetGameDtos().Count());
        }
    }
}