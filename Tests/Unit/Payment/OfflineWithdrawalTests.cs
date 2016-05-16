using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Exceptions;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalTests : AdminWebsiteUnitTestsBase
    {
        #region Fields

        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private ISecurityProvider _securityProvider;
        private IGameRepository _walletRepository;
        private WithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private IWagerConfigurationCommands _wageringConfigurationCommands;
        private Guid _gameId;
        private Core.Player.Data.Player _player;
        #endregion

        #region Methods

        public override void BeforeEach()
        {
            base.BeforeEach();

            _withdrawalService = Container.Resolve<WithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            _walletRepository = Container.Resolve<IGameRepository>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wageringConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();

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
        public void Can_create_OW_request()
        {
            _paymentTestHelper.MakeDeposit(_player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 10000, _player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            var requests = _paymentRepository.OfflineWithdraws.ToList();
            Assert.IsNotEmpty(requests);
        }

        [Test]
        public void Can_create_OW_with_1x_auto_wager_check()
        {
            //Make deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 100);

            //Make bet
            _gamesTestHelper.PlaceAndWinBet(20, 20,  _player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(30, 30,  _player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(50, 100, _player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(AutoWagerCheckException), ExpectedMessage = "Deposit wagering requirement has not been completed.")]
        public void Can_create_OW_with_1x_auto_wager_check_with_gap_between_deposits()
        {
            var player = _player;

            var wagerId = _wageringConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO()
            {
                BrandId = player.BrandId,
                IsDepositWageringCheck = true,
                Currency = player.CurrencyCode
            }, Guid.NewGuid());
            _wageringConfigurationCommands.ActivateWagerConfiguration(wagerId, Guid.NewGuid());
            //Make deposit
            _paymentTestHelper.MakeDeposit(player.Id, 1000);

            //Make bet
            _gamesTestHelper.PlaceAndWinBet(500, 600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(200, 600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(500, 600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(50,  600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id, _gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 200,
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

            //Make deposit
            _paymentTestHelper.MakeDeposit(player.Id, 500);

            //Make bet
            _gamesTestHelper.PlaceAndWinBet(300, 600, player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(100, 600, player.Id, _gameId);

            offlineWithdrawalRequest = new OfflineWithdrawRequest
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

            response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(NotEnoughFundsException), ExpectedMessage = "Entered amount exceeds the current balance.")]
        public void Cannot_create_OW_greater_than_limit_per_day()
        {
            var player = _player;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = 100,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 31,
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
        }

        [Test]
        [ExpectedException(typeof(NotEnoughFundsException), ExpectedMessage = "Entered amount exceeds the current balance.")]
        public void Cannot_create_OW_request_exceeding_transaction_limit()
        {
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 20,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 21,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(PaymentSettingsViolatedException), ExpectedMessage = "{\"text\": \"app:payment.settings.amountBelowAllowedValueError\", \"variables\": {\"value\": \"10.00\"}}")]
        public void Cannot_create_OW_lower_than_transaction_limit()
        {
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = 10,
                MaxAmountPerTransaction = 20,
                MaxAmountPerDay = 30,
                MaxTransactionPerDay = 40,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60,
                PaymentType = PaymentType.Withdraw
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(AutoWagerCheckException), ExpectedMessage = "Deposit wagering requirement has not been completed.")]
        public void Cannot_create_OW_with_auto_wager_check()
        {
            var wagerId = _wageringConfigurationCommands.CreateWagerConfiguration(new WagerConfigurationDTO()
            {
                BrandId = _player.BrandId,
                IsDepositWageringCheck = true,
                Currency = _player.CurrencyCode
            }, Guid.NewGuid());
            _wageringConfigurationCommands.ActivateWagerConfiguration(wagerId, Guid.NewGuid());
            //Make deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 200);
            //Make bet
            _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id, _gameId);
            _gamesTestHelper.PlaceAndWinBet(50, 10000, _player.Id, _gameId);

            //Make one more deposit
            _paymentTestHelper.MakeDeposit(_player.Id, 100);

            //Bet one more
            _gamesTestHelper.PlaceAndWinBet(20, 10000, _player.Id, _gameId);

            _walletRepository.SaveChanges();

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(offlineWithdrawalRequest);
        }

        [Test]
        [ExpectedException(typeof(NotEnoughFundsException), ExpectedMessage = "Entered amount exceeds the current balance.")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_day()
        {
            const int transactionsPerDay = 10;
            
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = _player.BrandId,
                VipLevel = _player.VipLevel.Id.ToString(),
                CurrencyCode = _player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = transactionsPerDay,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
                var offlineWithdrawalRequest = new OfflineWithdrawRequest
                {
                    Amount = 1,
                    NotificationType = NotificationType.None,
                    BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                    BankTime = _paymentRepository.Banks.First().Created.ToString(),
                    PlayerBankAccountId = _paymentRepository
                        .PlayerBankAccounts
                        .Include(x => x.Player)
                        .First(x => x.Player.Id == _player.Id)
                        .Id,
                    Remarks = "asd",
                    RequestedBy = _securityProvider.User.UserName
                };

                _withdrawalService.Request(offlineWithdrawalRequest);
            }

            var lastRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                    .PlayerBankAccounts
                    .Include(x => x.Player)
                    .First(x => x.Player.Id == _player.Id)
                    .Id,
                Remarks = "asd",
                RequestedBy = _securityProvider.User.UserName
            };

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        [ExpectedException(typeof(NotEnoughFundsException), ExpectedMessage = "Entered amount exceeds the current balance.")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_month()
        {
            const int transactionsPerMonth = 10;
            var player = _player;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = 100,
                MaxTransactionPerWeek = 50,
                MaxTransactionPerMonth = transactionsPerMonth
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
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

            var lastRequest = new OfflineWithdrawRequest
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

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        [ExpectedException(typeof(NotEnoughFundsException), ExpectedMessage = "Entered amount exceeds the current balance.")]
        public void Cannot_create_OWs_more_than_transaction_limit_per_week()
        {
            const int transactionPerWeek = 10;
            var player = _player;
            var paymentSettings = new PaymentSettings
            {
                Id = new Guid("50797305-5320-4086-BA1C-9D062DDF53D9"),
                BrandId = player.BrandId,
                VipLevel = player.VipLevel.Id.ToString(),
                CurrencyCode = player.CurrencyCode,
                MinAmountPerTransaction = -10,
                MaxAmountPerTransaction = 100,
                MaxAmountPerDay = 100,
                MaxTransactionPerDay = 100,
                MaxTransactionPerWeek = transactionPerWeek,
                MaxTransactionPerMonth = 60
            };
            _paymentRepository.PaymentSettings.Add(paymentSettings);

            for (int i = 0; i < 10; i++)
            {
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

            var lastRequest = new OfflineWithdrawRequest
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

            var response = _withdrawalService.Request(lastRequest);
        }

        [Test]
        public void Can_cancel_withdrawal_request()
        {
            var player = _player;
            _walletRepository.SaveChanges();
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

            var response = _withdrawalService.Request(offlineWithdrawalRequest);

            _withdrawalService.Cancel(response.Id, TestDataGenerator.GetRandomString(10));
            var ow = _withdrawalService.GetWithdrawalsCanceled();

            Assert.IsNotEmpty(ow);
            Assert.AreEqual(1, ow.Count());
        }
        #endregion
    }
}