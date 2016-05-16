using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class GamesTestHelper
    {
        private readonly IGameCommands _gameCommands;
        private readonly PlayerQueries _playerQueries;
        private readonly IGameRepository _gamesRepository;

        public GamesTestHelper(IGameCommands gameCommands, PlayerQueries playerQueries, IGameRepository gameRepository)
        {
            _gameCommands = gameCommands;
            _playerQueries = playerQueries;
            _gamesRepository = gameRepository;
        }

        public void PlaceAndWinBet(decimal amountPlaced, decimal amountWon, Guid playerId, Guid gameId)
        {
            var actualBetId = PlaceBet(amountPlaced, playerId, gameId);
            WinBet(actualBetId, amountWon);
        }

        public void WinBet(string roundId, decimal amountWon)
        {
            _gameCommands.WinBet(GameActionData.NewGameActionData(roundId, amountWon, "CAD", Guid.NewGuid()), new GameActionContext());
        }

        public void PlaceAndLoseBet(decimal amount, Guid playerId, Guid gameId)
        {
            var actualBetId = PlaceBet(amount, playerId, gameId);
            LoseBet(actualBetId);
        }

        public void LoseBet(string roundId)
        {
            _gameCommands.LoseBet(GameActionData.NewGameActionData(roundId, 0, "CAD", Guid.NewGuid()), new GameActionContext());
        }

        public void PlaceAndCancelBet(decimal amount, Guid playerId, Guid gameId)
        {
            var gameActionId = Guid.NewGuid().ToString();

            var actualBetId = PlaceBet(amount, playerId, gameId, transactionId:gameActionId);
            CancelBet(actualBetId, amount, gameActionId);
        }

        public void CancelBet(string roundId, decimal amount, string transactionIdToCancel)
        {
            var newBet = GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid());
            newBet.TransactionReferenceId = transactionIdToCancel;

            _gameCommands.AdjustTransaction(newBet, new GameActionContext());
        }

        public string PlaceBet(decimal amount, Guid playerId, Guid gameId, string roundId = null, string transactionId = null)
        {

            var token = new TokenData
            {
                GameId = gameId,
                PlayerId = playerId,
                BrandId = _playerQueries.GetPlayer(playerId).BrandId
            };

            roundId = roundId ?? Guid.NewGuid().ToString();
            _gameCommands.PlaceBet(
                GameActionData.NewGameActionData(roundId, amount, "CAD", Guid.NewGuid(), transactionId),
                new GameActionContext(), token);

            return roundId;
        }

        public GameProvider CreateGameProvider()
        {
            var gameProviderId = Guid.NewGuid();
            var gameProvider = new GameProvider
            {
                Id = gameProviderId,
                Name = TestDataGenerator.GetRandomString(),
                IsActive = true,
                GameProviderConfigurations = new Collection<GameProviderConfiguration>()
                {
                    new GameProviderConfiguration
                    {
                        Id = Guid.NewGuid(),
                        Endpoint = "http://localhost"
                    }    
                },
                Games = new List<Game>
                {
                    CreateGame(gameProviderId, TestDataGenerator.GetRandomString())
                }
            };
            _gamesRepository.GameProviders.Add(gameProvider);
            return gameProvider;
        }

        public GameProviderBetLimit CreateBetLevel(GameProvider gameProvider, Guid brandId)
        {
            var betLevel = new GameProviderBetLimit
            {
                Id = Guid.NewGuid(),
                GameProviderId = gameProvider.Id,
                BrandId = brandId,
                Code = new Random().Next(100000).ToString(),
                DateCreated = DateTimeOffset.UtcNow
            };
            _gamesRepository.BetLimits.Add(betLevel);
            return betLevel;
        }

        public Game CreateGame(Guid gameProviderId, string name)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                GameProviderId = gameProviderId,
                Name = name,
                EndpointPath = "/Game/Index"
            };
            _gamesRepository.Games.Add(game);
            return game;
        }

        public Guid GetGameId(Guid walletTemplateId)
        {

            var walletTemplate = _gamesRepository.WalletTemplates.Single(x => x.Id == walletTemplateId);

            var allProductIds = walletTemplate.WalletTemplateGameProviders.Select(x => x.GameProviderId).ToList();
            var productId = allProductIds.First();

            var games =
                _gamesRepository.GameProviders.Single(x => x.Id == productId).Games;
            return games.ToList().First().Id;

        }


        public Guid GetGameId(Core.Brand.Data.WalletTemplate walletTemplate)
        {
            var allProductIds = walletTemplate.WalletTemplateProducts.Select(x => x.ProductId).ToList();
            var productId = allProductIds.First();

            var games =
                _gamesRepository.GameProviders.Single(x => x.Id == productId).Games;
            return games.ToList().First().Id;

        }
    }
}