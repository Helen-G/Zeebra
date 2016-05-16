using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalValidationTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private GamesTestHelper _gamesTestHelper;
        private FakePaymentRepository _paymentRepository;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;
        private ISecurityProvider _securityProvider;
        private WithdrawalService _withdrawalService;
        private IGameRepository _walletRepository;
        private Mock<IAWCValidationService> mockAwcChec;
        private Mock<IBonusWageringWithdrawalValidationService> mockBonusWageringCheck;
        private Mock<IFundsValidationService> mockFundsValidationCheck;
        private Mock<IManualAdjustmentWageringValidationService> mockManualAdjWageringCheck;
        private Mock<IPaymentSettingsValidationService> mockPaymentSettingsCheck;
        private Mock<IRebateWageringValidationService> mockRebateValidationCheck;
        private Guid _gameId;
        private Core.Player.Data.Player _player;

        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();
            mockAwcChec = new Mock<IAWCValidationService>();
            Container.RegisterInstance(mockAwcChec.Object);
            mockBonusWageringCheck = new Mock<IBonusWageringWithdrawalValidationService>();
            Container.RegisterInstance(mockBonusWageringCheck.Object);
            mockPaymentSettingsCheck = new Mock<IPaymentSettingsValidationService>();
            Container.RegisterInstance(mockPaymentSettingsCheck.Object);
            mockManualAdjWageringCheck = new Mock<IManualAdjustmentWageringValidationService>();
            Container.RegisterInstance(mockManualAdjWageringCheck.Object);
            mockRebateValidationCheck = new Mock<IRebateWageringValidationService>();
            Container.RegisterInstance(mockRebateValidationCheck.Object);
            mockFundsValidationCheck = new Mock<IFundsValidationService>();
            Container.RegisterInstance(mockFundsValidationCheck.Object);
            _withdrawalService = Container.Resolve<WithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _walletRepository = Container.Resolve<IGameRepository>();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BonusWorker>().Start();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            brandHelper.CreateActiveBrandWithProducts();

            var playerId = Container.Resolve<PlayerTestHelper>().CreatePlayer(null);
            _paymentTestHelper.CreatePlayerBankAccount(playerId, true);

            _player = _playerQueries.GetPlayers().ToList().First();

            _gameId = brandHelper.GetMainWalletGameId(playerId);
        }

        [Test]
        public void Withdrawal_request_runs_auto_wager_check_validation()
        {
            CreateOWR();
            mockAwcChec.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public void Withdrawal_request_runs_bonus_wagering_check_validation()
        {
            CreateOWR();
            mockBonusWageringCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public void Withdrawal_request_runs_funds_check_validation()
        {
            CreateOWR();
            mockFundsValidationCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public void Withdrawal_request_runs_manual_adj_wagering_check_validation()
        {
            CreateOWR();
            mockManualAdjWageringCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public void Withdrawal_request_runs_payment_settings_check_validation()
        {
            CreateOWR();
            mockPaymentSettingsCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        [Test]
        public void Withdrawal_request_runs_rebate_wagering_check_validation()
        {
            CreateOWR();
            mockRebateValidationCheck.Verify(n =>
                n.Validate(It.IsAny<OfflineWithdrawRequest>()),
                Times.AtLeastOnce());
        }

        private void CreateOWR()
        {
            var player = _player;
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 10000, player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
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
        }

        #endregion
    }
}