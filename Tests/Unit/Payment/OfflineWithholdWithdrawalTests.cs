using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalWithholdTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private ISecurityProvider _securityProvider;
        private WithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private WalletQueries _walletQueries;
        private GameRepository _walletRepository;
        private Guid _gameId;
        #endregion

        public override void BeforeEach()
        {
            base.BeforeEach();

            _withdrawalService = Container.Resolve<WithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _walletQueries = Container.Resolve<WalletQueries>();
            _walletRepository = Container.Resolve<GameRepository>();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BonusWorker>().Start();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            brandHelper.CreateActiveBrandWithProducts();

            var playerId = Container.Resolve<PlayerTestHelper>().CreatePlayer(null);
            _paymentTestHelper.CreatePlayerBankAccount(playerId, true);
            _gameId = brandHelper.GetMainWalletGameId(playerId);

        }

        [Test]
        public void Can_withhold_withdrawal_money()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 1100, player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1000,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            _withdrawalService.Request(offlineWithdrawalRequest);

            var playerBalance = _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(100, playerBalance.Free);
        }


        [Test]
        public void Can_revert_withhold_withdrawal_money_after_withdrawal_canceletion()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _walletRepository.SaveChanges();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 1100, player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1000,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            var playerBalance = _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(100, playerBalance.Free);

            _withdrawalService.Cancel(
                response.Id,
                TestDataGenerator.GetRandomString());

            playerBalance = _walletQueries.GetPlayerBalance(player.Id);
            Assert.AreEqual(1100, playerBalance.Free);
        }
    }
}
