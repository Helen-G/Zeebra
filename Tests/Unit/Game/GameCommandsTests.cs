using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Game
{
    internal class GamesCommandsTests : AdminWebsiteUnitTestsBase
    {
        private IGameCommands _commands;
        private Mock<IEventBus> _eventBusMock;
        private Mock<IGameWalletOperations> _gameWalletsOperationsMock;
        private FakeGameRepository _repository;

        private GameActionContext _GameActionContext 
        {
            get { return new GameActionContext {GameProviderId = Guid.NewGuid()}; }
        }

        public override void BeforeEach()
        {
            base.BeforeEach();

            _repository = new FakeGameRepository();
            _eventBusMock = new Mock<IEventBus>();
            _gameWalletsOperationsMock = new Mock<IGameWalletOperations>();

            _gameWalletsOperationsMock.Setup(
                t => t.PlaceBet(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Guid.NewGuid());
            _commands = new GameCommands(_repository, _gameWalletsOperationsMock.Object, _eventBusMock.Object);
        }

        [Test]
        public void Can_Place_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId); 
            var token = GenerateRandomTokenData(tokenId);

            // Act
            _commands.PlaceBet(placeBetAction, _GameActionContext, token);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(token.PlayerId);
            actualRound.Data.BrandId.Should().Be(token.BrandId);
            actualRound.Data.GameId.Should().Be(token.GameId);
            actualRound.Data.Status.Should().Be(RoundStatus.Open);
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Should().NotBeEmpty();

            var actualGameAction = actualRound.Data.GameActions[0];

            actualGameAction.TokenId.Should().Be(tokenId);
            actualGameAction.GameActionType.Should().Be(GameActionType.Placed);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(placeBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(placeBetAction.Description);
            actualGameAction.Amount.Should().Be(-placeBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);
            
            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlaced>()));
        }

        [Test]
        public void Can_Place_Bet_Twice()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            // Act
            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var secondPlaceBetAction = GenerateRandomGameAction(tokenId);
            secondPlaceBetAction.RoundId = placeBetAction.RoundId;
            secondPlaceBetAction.Amount = 20;
            _commands.PlaceBet(secondPlaceBetAction, _GameActionContext, token);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(token.PlayerId);
            actualRound.Data.BrandId.Should().Be(token.BrandId);
            actualRound.Data.GameId.Should().Be(token.GameId);
            actualRound.Data.Status.Should().Be(RoundStatus.Open);
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount + secondPlaceBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualGameAction = actualRound.Data.GameActions[1];

            actualGameAction.TokenId.Should().Be(tokenId);
            actualGameAction.GameActionType.Should().Be(GameActionType.Placed);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(secondPlaceBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(secondPlaceBetAction.Description);
            actualGameAction.Amount.Should().Be(-secondPlaceBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);

            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlaced>()));
        }

        
        [Test]
        public void Cannot_Place_Duplicate_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var externalTransactionId = Guid.NewGuid().ToString();

            // add a transaction
            _repository.Rounds.Add(new Round 
            {
                GameActions = new List<GameAction>
                {
                    new GameAction
                    {
                        ExternalTransactionId = externalTransactionId
                    }
                }
            });

            var placeBetAction = GenerateRandomGameAction(tokenId, externalTransactionId);
            var token = GenerateRandomTokenData(tokenId);

            // Act
            Action act = () => _commands.PlaceBet(placeBetAction, _GameActionContext, token);

            // Assert
            act.ShouldThrow<DuplicateGameActionException>();
        }

        [Test]
        public void Can_Win_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var winBetAction = GenerateRandomGameAction(tokenId, Guid.NewGuid().ToString());

            winBetAction.RoundId = placeBetAction.RoundId;
            winBetAction.Amount = 25;

            _gameWalletsOperationsMock.Setup(
                t => t.WinBet(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.WinBet(winBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(winBetAction.Amount);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualWinBetGameAction = actualRound.Data.GameActions[1];

            actualWinBetGameAction.TokenId.Should().Be(tokenId);
            actualWinBetGameAction.GameActionType.Should().Be(GameActionType.Won);
            actualWinBetGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualWinBetGameAction.ExternalTransactionId.Should().Be(winBetAction.ExternalTransactionId);
            actualWinBetGameAction.ExternalBatchId.Should().BeNull();
            actualWinBetGameAction.Description.Should().Be(winBetAction.Description);
            actualWinBetGameAction.Amount.Should().Be(winBetAction.Amount);
            actualWinBetGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetWon>()));
        }

        [Test]
        public void Cannot_Win_NonExisting_Bet()
        {
            // Arrange
            var winBetAction = GenerateRandomGameAction(Guid.NewGuid());

            // Act
            Action act = () => _commands.WinBet(winBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }

        [Test]
        public void Can_Lose_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var loseBetAction = GenerateRandomGameAction(tokenId, Guid.NewGuid().ToString());

            loseBetAction.RoundId = placeBetAction.RoundId;
            loseBetAction.Amount = 0;

            _gameWalletsOperationsMock.Setup(
                t => t.LoseBet(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.LoseBet(loseBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            
            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualLoseBetGameAction = actualRound.Data.GameActions[1];

            actualLoseBetGameAction.TokenId.Should().Be(tokenId);
            actualLoseBetGameAction.GameActionType.Should().Be(GameActionType.Lost);
            actualLoseBetGameAction.WalletTransactionId.Should().BeEmpty();
            actualLoseBetGameAction.ExternalTransactionId.Should().Be(loseBetAction.ExternalTransactionId);
            actualLoseBetGameAction.ExternalBatchId.Should().BeNull();
            actualLoseBetGameAction.Description.Should().Be(loseBetAction.Description);
            actualLoseBetGameAction.Amount.Should().Be(loseBetAction.Amount);
            actualLoseBetGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetLost>()));
        }

        [Test]
        public void Cannot_Lose_Bet_With_Nonzero_Amount()
        {
            // Arrange
            var loseBetAction = GenerateRandomGameAction(Guid.NewGuid(), Guid.NewGuid().ToString());
            loseBetAction.Amount = 10; // non-zero
            
            // Act
            Action act = () => _commands.LoseBet(loseBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<LoseBetAmountMustBeZeroException>();
        }

        [Test]
        public void Cannot_Lose_Nonexisting_Bet()
        {
            // Arrange
            var loseBetAction = GenerateRandomGameAction(Guid.NewGuid(), Guid.NewGuid().ToString());
            loseBetAction.Amount = 0; // must be zero

            // Act
            Action act = () => _commands.LoseBet(loseBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }

        [Test]
        public void Can_Free_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var freeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);


            _gameWalletsOperationsMock.Setup(
                t => t.FreeBet(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.FreeBet(freeBetAction, _GameActionContext, token);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == freeBetAction.RoundId);

            actualRound.Should().NotBeNull();
            actualRound.Data.PlayerId.Should().Be(token.PlayerId);
            actualRound.Data.BrandId.Should().Be(token.BrandId);
            actualRound.Data.GameId.Should().Be(token.GameId);
            actualRound.Data.Status.Should().Be(RoundStatus.Closed);
            actualRound.WonAmount.Should().Be(freeBetAction.Amount);
            actualRound.AdjustedAmount.Should().Be(0);
            actualRound.Amount.Should().Be(0);
            actualRound.Data.GameActions.Should().NotBeEmpty();

            var actualGameAction = actualRound.Data.GameActions[0];

            actualGameAction.TokenId.Should().Be(tokenId);
            actualGameAction.GameActionType.Should().Be(GameActionType.Free);
            actualGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualGameAction.ExternalTransactionId.Should().Be(freeBetAction.ExternalTransactionId);
            actualGameAction.ExternalBatchId.Should().BeNull();
            actualGameAction.Description.Should().Be(freeBetAction.Description);
            actualGameAction.Amount.Should().Be(freeBetAction.Amount);
            actualGameAction.Round.Id.Should().Be(actualRound.Data.Id);

            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetPlacedFree>()));
        }

        [Test]
        public void Can_Adjust_Bet()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var adjustingAction = GenerateRandomGameAction(tokenId, Guid.NewGuid().ToString());

            adjustingAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;
            adjustingAction.RoundId = placeBetAction.RoundId;
            adjustingAction.Amount = placeBetAction.Amount + 20;



            _gameWalletsOperationsMock.Setup(
                t => t.AdjustBetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.AdjustTransaction(adjustingAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(adjustingAction.Amount);
            actualRound.Amount.Should().Be( placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualAdjustmentGameAction = actualRound.Data.GameActions[1];

            actualAdjustmentGameAction.TokenId.Should().Be(tokenId);
            actualAdjustmentGameAction.GameActionType.Should().Be(GameActionType.Adjustment);
            actualAdjustmentGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualAdjustmentGameAction.ExternalTransactionId.Should().Be(adjustingAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalBatchId.Should().BeNull();
            actualAdjustmentGameAction.Description.Should().Be(adjustingAction.Description);
            actualAdjustmentGameAction.Amount.Should().Be(adjustingAction.Amount);
            actualAdjustmentGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetAdjusted>()));
        }

        [Test]
        public void Can_Adjust_BetTransaction()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var adjustingBetAction = GenerateRandomGameAction(tokenId, Guid.NewGuid().ToString());

            adjustingBetAction.RoundId = placeBetAction.RoundId;
            adjustingBetAction.Amount = placeBetAction.Amount + 20;
            adjustingBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;

            _gameWalletsOperationsMock.Setup(
                t => t.AdjustBetTransaction(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>()))
                .Returns(Guid.NewGuid());
            // Act
            _commands.AdjustTransaction(adjustingBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(adjustingBetAction.Amount);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualAdjustmentGameAction = actualRound.Data.GameActions[1];

            actualAdjustmentGameAction.TokenId.Should().Be(tokenId);
            actualAdjustmentGameAction.GameActionType.Should().Be(GameActionType.Adjustment);
            actualAdjustmentGameAction.WalletTransactionId.Should().NotBeEmpty();
            actualAdjustmentGameAction.ExternalTransactionId.Should().Be(adjustingBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualAdjustmentGameAction.ExternalBatchId.Should().BeNull();
            actualAdjustmentGameAction.Description.Should().Be(adjustingBetAction.Description);
            actualAdjustmentGameAction.Amount.Should().Be(adjustingBetAction.Amount);
            actualAdjustmentGameAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetAdjusted>()));
        }

        [Test]
        public void Cannot_Adjust_Nonexisting_Bet()
        {
            // Arrange
            var adjustingBetAction = GenerateRandomGameAction(Guid.NewGuid());

            // Act
            Action act = () => _commands.AdjustTransaction(adjustingBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        }



        [Test]
        public void Can_Cancel_BetTransaction()
        {
            // Arrange
            var tokenId = Guid.NewGuid();
            var placeBetAction = GenerateRandomGameAction(tokenId);
            var token = GenerateRandomTokenData(tokenId);

            _commands.PlaceBet(placeBetAction, _GameActionContext, token); // place initial bet

            var cancelBetAction = GenerateRandomGameAction(tokenId, Guid.NewGuid().ToString());

            cancelBetAction.RoundId = placeBetAction.RoundId;
            cancelBetAction.TransactionReferenceId = placeBetAction.ExternalTransactionId;


            _gameWalletsOperationsMock.Setup(
                t => t.CancelBet(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Guid.NewGuid());

            // Act
            _commands.CancelTransaction(cancelBetAction, _GameActionContext);

            // Assert
            var actualRound = _repository.GetRound(x => x.ExternalRoundId == placeBetAction.RoundId);

            actualRound.WonAmount.Should().Be(0);
            actualRound.AdjustedAmount.Should().Be(cancelBetAction.Amount);
            actualRound.Amount.Should().Be(placeBetAction.Amount);
            actualRound.Data.GameActions.Count.Should().Be(2);

            var actualCancelBetAction = actualRound.Data.GameActions[1];

            actualCancelBetAction.TokenId.Should().Be(tokenId);
            actualCancelBetAction.GameActionType.Should().Be(GameActionType.Cancel);
            actualCancelBetAction.WalletTransactionId.Should().NotBeEmpty();
            actualCancelBetAction.ExternalTransactionId.Should().Be(cancelBetAction.ExternalTransactionId);
            actualCancelBetAction.ExternalTransactionReferenceId.Should().Be(placeBetAction.ExternalTransactionId);
            actualCancelBetAction.ExternalBatchId.Should().BeNull();
            actualCancelBetAction.Description.Should().Be(cancelBetAction.Description);
            actualCancelBetAction.Amount.Should().Be(placeBetAction.Amount);
            actualCancelBetAction.Round.Id.Should().Be(actualRound.Data.Id);


            _eventBusMock.Verify(x => x.Publish(It.IsAny<BetCancelled>()));
        }

        [Test]
        public void Cannot_Cancel_Nonexisting_Bet()
        {
            // Arrange
            var cancelBetAction = GenerateRandomGameAction(Guid.NewGuid());

            // Act
            Action act = () => _commands.AdjustTransaction(cancelBetAction, _GameActionContext);

            // Assert
            act.ShouldThrow<RoundNotFoundException>();
        } 





        // Helpers
        private GameActionData GenerateRandomGameAction(Guid tokenId, string externalTxId = null)
        {
            return new GameActionData
            {
                Amount = 10,
                RoundId = Guid.NewGuid().ToString(),
                CurrencyCode = TestDataGenerator.GetRandomAlphabeticString(3),
                Description = TestDataGenerator.GetRandomAlphabeticString(100),
                ExternalTransactionId = externalTxId ?? Guid.NewGuid().ToString(),
                TokenId = tokenId
            };
        }

        private TokenData GenerateRandomTokenData(Guid tokenId)
        {
            return new TokenData
            {
                BrandId = Guid.NewGuid(),
                CurrencyCode = TestDataGenerator.GetRandomAlphabeticString(3),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = "0.1.2.3",
                TokenId = tokenId
            };
        }

    }
}
