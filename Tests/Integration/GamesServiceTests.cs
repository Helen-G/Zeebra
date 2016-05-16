using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class GamesServiceTests : WebServiceTestsBase
    {
        private IGameRepository _repository;
        private IGameCommands _gameCommands;
        private IGameQueries _gameQueries;
        private readonly Guid _brandId = new Guid("00000000-0000-0000-0000-000000000138");
        private Guid _playerId;
        private Guid _gameId;
        private IEventRepository _eventRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            Container.Resolve<SecurityTestHelper>().SignInSuperAdmin();

            _eventRepository = Container.Resolve<EventRepository>();
            _repository = Container.Resolve<GameRepository>();
            _gameCommands = Container.Resolve<GameCommands>();
            _gameQueries = Container.Resolve<GameQueries>();

            var player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            WaitForPlayerRegistered(player.Id, TimeSpan.FromSeconds(20));
            _playerId = player.Id;

            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var productWalletTemplateId = Container.Resolve<BrandTestHelper>().GetProductWalletTemplate(_brandId).Id;

            paymentTestHelper.MakeDeposit(player.Id, 1000);
            paymentTestHelper.MakeFundIn(player.Id, productWalletTemplateId, 1000);

        }


        [Test]
        public void Can_Generate_BetPlaced_Event()
        {
            const int amount = 100;
            Guid placedGameActionId;

            PlaceBet(amount, out placedGameActionId);

            var round = _gameQueries.GetRoundByGameActionId(placedGameActionId);
            
            var @event = _eventRepository.GetEvents<BetPlaced>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(0));
            Assert.That(@event.WonAmount, Is.EqualTo(0));
            Assert.That(@event.Amount, Is.EqualTo(amount));
        }

        [Test]
        public void Can_Generate_BetPlacedFree_Event()
        {
            const int amount = 100;

            var roundId = Guid.NewGuid().ToString();

            var placedGameActionId = _gameCommands.FreeBet(
                GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid()),
                new GameActionContext(), GetToken());

            var round = _gameQueries.GetRoundByGameActionId(placedGameActionId);

            var @event = _eventRepository.GetEvents<BetPlacedFree>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(0));
            Assert.That(@event.WonAmount, Is.EqualTo(amount));
            Assert.That(@event.Amount, Is.EqualTo(0));
        }

        [Test]
        public void Can_Generate_BetWon_Event()
        {
            const int amount = 100;
            const int winAmount = 200;
            Guid placedBetGameActionId;

            WinBet(winAmount, PlaceBet(amount, out placedBetGameActionId));

            var round = _gameQueries.GetRoundByGameActionId(placedBetGameActionId);

            var @event = _eventRepository.GetEvents<BetWon>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(0));
            Assert.That(@event.WonAmount, Is.EqualTo(winAmount));
            Assert.That(@event.Amount, Is.EqualTo(amount));
        }


        [Test]
        public void Can_Generate_BetLost_Event()
        {
            const int amount = 100;
            Guid placedBetGameActionId;

            _gameCommands.LoseBet(
                GameActionData.NewGameActionData(PlaceBet(amount, out placedBetGameActionId), 0, "CAD", Guid.NewGuid()),
                new GameActionContext());
            
            var round = _gameQueries.GetRoundByGameActionId(placedBetGameActionId);

            var @event = _eventRepository.GetEvents<BetLost>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(0));
            Assert.That(@event.WonAmount, Is.EqualTo(0));
            Assert.That(@event.Amount, Is.EqualTo(amount));
        }


        [Test]
        public void Can_Generate_BetCancel_Event()
        {
            const int amount = 100;

            var token = GetToken();
            var roundId = Guid.NewGuid().ToString();
            var extTxId = Guid.NewGuid().ToString();
            var gameActionData = GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid(), externalTransactionId: extTxId);

            var placedGameActionId = _gameCommands.PlaceBet(gameActionData, new GameActionContext(), token);

            _gameCommands.CancelTransaction(
                GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid(), transactionReferenceId: extTxId), 
                new GameActionContext());

            var round = _gameQueries.GetRoundByGameActionId(placedGameActionId);

            var @event = _eventRepository.GetEvents<BetCancelled>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event, Is.Not.Null);
            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(amount));
            Assert.That(@event.WonAmount, Is.EqualTo(0));
            Assert.That(@event.Amount, Is.EqualTo(amount));
        }


        [Test]
        public void Can_Generate_BetAdjusted_Event()
        {
            const int amount = 100;
            const int adjustingAmount = 50;
            
            var token = GetToken();

            var roundId = Guid.NewGuid().ToString();
            var extTxId = Guid.NewGuid().ToString();
            var gameActionData = GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid(), externalTransactionId: extTxId);

            var placedBetGameActionId = _gameCommands.PlaceBet(gameActionData, new GameActionContext(), token);

            _gameCommands.AdjustTransaction(
                GameActionData.NewGameActionData(roundId, adjustingAmount, "CAD", Guid.NewGuid(), transactionReferenceId: extTxId),
                new GameActionContext());

            var round = _gameQueries.GetRoundByGameActionId(placedBetGameActionId);

            var @event = _eventRepository.GetEvents<BetAdjusted>().SingleOrDefault(e => e.RoundId == round.Data.Id);

            Assert.That(@event, Is.Not.Null);

            Assert.That(@event.PlayerId, Is.EqualTo(_playerId));
            Assert.That(@event.BrandId, Is.EqualTo(_brandId));
            Assert.That(@event.GameId, Is.EqualTo(_gameId));
            Assert.That(@event.AdjustedAmount, Is.EqualTo(adjustingAmount));
            Assert.That(@event.WonAmount, Is.EqualTo(0));
            Assert.That(@event.Amount, Is.EqualTo(amount));
        }

        private TokenData GetToken()
        {
            _gameId = new Guid("C17F4D3F-2F99-42A4-A766-4493EFF6DB9F"); // ROULETTE 

            var token = new TokenData
            {
                GameId = _gameId,
                PlayerId = _playerId,
                BrandId = _brandId
            };
            return token;
        }

        private string PlaceBet(decimal amount, out Guid placedGameActionId)
        {
            var token = GetToken();
            var roundId = Guid.NewGuid().ToString();

            placedGameActionId = _gameCommands.PlaceBet(
                GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid()),
                new GameActionContext(), token);

            return roundId;

        }

        private void WinBet(decimal amount, string roundId)
        {
            _gameCommands.WinBet(
                GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid()),
                new GameActionContext());
        }

        private void WaitForPlayerRegistered(Guid playerId, TimeSpan timeout)
        {
            var stopwatch = Stopwatch.StartNew();
            while (_repository.Players.All(p => p.Id != playerId) && stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(100);
            }
            if (_repository.Players.All(p => p.Id != playerId))
            {
                throw new RegoException("Player registration timeout");
            }
        }
    }

}
