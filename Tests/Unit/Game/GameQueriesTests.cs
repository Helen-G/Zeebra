using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;


namespace AFT.RegoV2.Tests.Unit.Game
{

    internal class GameQueriesTests : AdminWebsiteUnitTestsBase
    {
        private IGameRepository _repository { get; set; }
        private IGameQueries _queries { get; set; }
        private readonly Guid _brandId = Guid.NewGuid();
        private static Random random = new Random();


        public override void BeforeEach()
        {

            base.BeforeEach();

            _repository = Container.Resolve<FakeGameRepository>();
            _queries = Container.Resolve<GameQueries>();

            PopulateFakeData();
            
        }

        [Test]
        public void Can_Get_Playable_Balance()
        {
            var amount = random.Next(0,1000);


            var wallets = _repository.Wallets.ToList();
            var i = random.Next(1, wallets.Count - 1);
            var randomWallet = wallets[i];

            randomWallet.Main = amount;
            randomWallet.Bonus = amount/2;

            var playerId = randomWallet.PlayerId;
            var games = randomWallet.Template.WalletTemplateGameProviders.First().GameProvider.Games.ToList();
            
            var randomGameIndex = random.Next(0, games.Count() - 1);
            var gameId = games[randomGameIndex].Id;

            var pamount = _queries.GetPlayableBalance(playerId, gameId);

            Assert.AreEqual(randomWallet.Playable, pamount);
        }

        #region Preparation
        private void PopulateFakeData()
        {
            FakeGameProviders();
            FakePlayers();

        }

        private void FakePlayers(int count = 5)
        {
            FakeWalletTemplates();
            for (var i = 0; i < count; i += 1)
            {
                var id = Guid.NewGuid();
                _repository.Players.Add(new Core.Game.Data.Player
                {
                    Id = id,
                    BrandId = _brandId,
                    Name = Guid.NewGuid().ToString().Substring(0, 4)
                });
                FakeWalletsForPlayer(id);
            }
        }

        private void FakeWalletTemplates(int count=2)
        {
            for (var i = 0; i < count; i += 1)
            {
                var id = Guid.NewGuid();

                _repository.WalletTemplates.Add(new WalletTemplate
                {
                    Id = id,
                    IsMain = i==0,
                    WalletTemplateGameProviders = FakeWalletTemplateGameProviders(id, i)
                });
            }
        }

        private List<WalletTemplateGameProvider> FakeWalletTemplateGameProviders(Guid wtid, int gpIndex)
        {
            var result = new List<WalletTemplateGameProvider>();

            var gp = _repository.GameProviders.ToList()[gpIndex];

            var wtgp = new WalletTemplateGameProvider
            {
                Id = Guid.NewGuid(),
                WalletTemplateId = wtid,
                GameProvider = gp,
                GameProviderId = gp.Id
            };
            _repository.WalletTemplateGameProviders.Add(wtgp);
            result.Add(wtgp);

            return result;
        }

        private void FakeWalletsForPlayer(Guid playerId)
        {
            var templates = _repository.WalletTemplates.ToList();
            foreach (var walletTemplate in templates)
            {
                _repository.Wallets.Add(new Core.Game.Data.Wallet
                {
                    Id = Guid.NewGuid(),
                    Main = 0,
                    Bonus = 0,
                    PlayerId = playerId,
                    Template = walletTemplate
                });
            }
        }

        private void FakeGameProviders(int count = 5)
        {
            for (var i = 0; i < count; i += 1)
            {
                var id = Guid.NewGuid();
                _repository.GameProviders.Add(new GameProvider
                {
                    Id = id,
                    Name = Guid.NewGuid().ToString().Substring(0, 3),
                    Games = FakeGames(id)
                });
                
            }
        }

        private List<Core.Game.Data.Game> FakeGames(Guid gpid, int count = 5)
        {
            var result = new List<Core.Game.Data.Game>();

            for (var i = 0; i < count; i += 1)
            {
                var g = new Core.Game.Data.Game
                {
                    Id = Guid.NewGuid(),
                    GameProviderId = gpid,
                    Name = Guid.NewGuid().ToString().Substring(0, 6)
                };
                result.Add(g);
                _repository.Games.Add(g);
            }
            return result;
        }
        #endregion
    }
}
