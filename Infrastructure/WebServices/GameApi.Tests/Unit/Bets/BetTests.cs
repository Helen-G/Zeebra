using System;
using AFT.RegoV2.Core.Common.Exceptions;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.Shared;
using NUnit.Framework;
using Round = AFT.RegoV2.Core.Game.Entities.Round;

namespace AFT.RegoV2.GameApi.Tests.Unit.Bets
{
    [Category("Unit")]
    internal class BetTests
    {
        const string PlayerIpAddress = "127.0.0.0";

        [Test]
        public void Can_Create_Round()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);

            Assert.AreEqual(tokenData.BrandId, round.Data.BrandId);
            Assert.AreEqual(tokenData.GameId, round.Data.GameId);
            Assert.AreEqual(tokenData.PlayerId, round.Data.PlayerId);
        }

        [Test]
        public void Can_Place_Bet()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Place(1234.56m, "test", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());

            Assert.AreEqual(1, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);

            var gameAction = round.Data.GameActions[0];

            Assert.AreEqual(-1234.56m, gameAction.Amount);
            Assert.AreEqual("test", gameAction.Description);
            Assert.AreEqual(GameActionType.Placed, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        public void Can_Win()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Place(1234.56m, "test", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());

            walletTxId = Guid.NewGuid();
            round.Win(10000m, "won", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());


            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);
            Assert.AreEqual(10000m, round.WonAmount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(10000m, gameAction.Amount);
            Assert.AreEqual("won", gameAction.Description);
            Assert.AreEqual(GameActionType.Won, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        public void Can_Free_Bet()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Free(10000m, "freebet", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());


            Assert.AreEqual(1, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
            Assert.AreEqual(0, round.Amount);
            Assert.AreEqual(10000m, round.WonAmount);

            var gameAction = round.Data.GameActions[0];

            Assert.AreEqual(10000m, gameAction.Amount);
            Assert.AreEqual("freebet", gameAction.Description);
            Assert.AreEqual(GameActionType.Free, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        public void Can_Lose()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };
            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Place(1234.56m, "test", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());

            round.Lose(String.Empty, Guid.NewGuid(), Guid.NewGuid().ToString());

            Assert.AreEqual(RoundStatus.Closed, round.Data.Status);
        }

        [Test]
        [ExpectedException(typeof(RegoException))]
        public void Can_Adjust_New_GameAction()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Adjust(-100m, "adjust", walletTxId, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(RegoException))]
        public void Can_Adjust_Nonexisting_GameAction()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Adjust(100m, "adjust", walletTxId, Guid.NewGuid());
        }

        [Test]
        public void Can_Adjust_Existing_GameAction()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Place(1234.56m, "test", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());

            walletTxId = Guid.NewGuid();
            round.Adjust(1000m, "adjust", walletTxId, Guid.NewGuid(), Guid.NewGuid().ToString());


            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);
            Assert.AreEqual(1000m, round.AdjustedAmount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(1000m, gameAction.Amount);
            Assert.AreEqual("adjust", gameAction.Description);
            Assert.AreEqual(GameActionType.Adjustment, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }

        [Test]
        [ExpectedException(typeof(GameActionNotFoundException))]
        public void Can_Cancel_Nonexisting_GameAction()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            round.Cancel(100m, "cancel", walletTxId, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), tokenData.TokenId);
        }

        [Test]
        public void Can_Cancel_Existing_GameAction()
        {
            var tokenData = new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = DateTime.UtcNow.ToUnixTimeSeconds(),
                BrandId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid(),
                PlayerIpAddress = PlayerIpAddress
            };

            var round = new Round(Guid.NewGuid().ToString(), tokenData);
            var walletTxId = Guid.NewGuid();

            var placeBetTxId = Guid.NewGuid().ToString();

            round.Place(1234.56m, "test", walletTxId, Guid.NewGuid(), placeBetTxId);

            walletTxId = Guid.NewGuid();
            round.Cancel(1234.56m, "cancel", walletTxId, Guid.NewGuid().ToString(), placeBetTxId, tokenData.TokenId);

            Assert.AreEqual(2, round.Data.GameActions.Count);
            Assert.AreEqual(RoundStatus.Open, round.Data.Status);
            Assert.AreEqual(1234.56m, round.Amount);

            var gameAction = round.Data.GameActions[1];

            Assert.AreEqual(1234.56, gameAction.Amount);
            Assert.AreEqual("cancel", gameAction.Description);
            Assert.AreEqual(GameActionType.Cancel, gameAction.GameActionType);
            Assert.AreEqual(walletTxId, gameAction.WalletTransactionId);
        }
    }

}