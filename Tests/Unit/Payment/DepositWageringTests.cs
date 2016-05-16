using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class DepositWageringTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private FakeBrandRepository _brandRepository;
        private GamesTestHelper _gamesTestHelper;
        private FakePaymentRepository _paymentRepository;
        private PaymentTestHelper _paymentTestHelper;
        private PlayerQueries _playerQueries;
        private Guid _gameId;

        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();

            _playerQueries = Container.Resolve<PlayerQueries>();
            _brandRepository = Container.Resolve<FakeBrandRepository>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();

            Container.Resolve<SecurityTestHelper>().SignInUser();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            brandHelper.CreateActiveBrandWithProducts();

            var playerId = Container.Resolve<PlayerTestHelper>().CreatePlayer(null);
            _gameId = brandHelper.GetMainWalletGameId(playerId);
        }

        [Test]
        public void Wagering_amount_setted_sucessfully()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 1000);
        }

        [Test]
        public void Wagering_amount_recalculated_sucessfully_while_player_bets()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(100, 100, player.Id, _gameId);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 900);
        }

        [Test]
        public void Cannot_make_wagering_requirement_less_than_zero()
        {
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 200);
            _gamesTestHelper.PlaceAndWinBet(50, 100, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(200, 100, player.Id, _gameId);

            var deposit = _paymentRepository.OfflineDeposits.FirstOrDefault(x => x.PlayerId == player.Id);

            Assert.NotNull(deposit);
            Assert.AreEqual(deposit.DepositWagering, 0);
        }

        #endregion
    }
}