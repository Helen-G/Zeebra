using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Exceptions;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using AFT.RegoV2.Tests.Unit.Fraud;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class OfflineWithdrawalAutoVerificationTests : AdminWebsiteUnitTestsBase
    {
        private FakePaymentRepository _paymentRepository;
        private PlayerQueries _playerQueries;
        private ISecurityProvider _securityProvider;
        private FakeGameRepository _gameRepository;
        private WithdrawalService _withdrawalService;
        private PaymentTestHelper _paymentTestHelper;
        private GamesTestHelper _gamesTestHelper;
        private IWagerConfigurationCommands _wageringConfigurationCommands;
        private IAVCConfigurationQueries _avcConfigurationQueries;
        private IAVCConfigurationCommands _avcConfigurationCommands;
        private BrandQueries _brandQueries;
        private Guid _gameId;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _withdrawalService = Container.Resolve<WithdrawalService>();
            _playerQueries = Container.Resolve<PlayerQueries>();
            _paymentRepository = Container.Resolve<FakePaymentRepository>();
            _gameRepository = Container.Resolve<FakeGameRepository>();
            _securityProvider = Container.Resolve<ISecurityProvider>();
            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _gamesTestHelper = Container.Resolve<GamesTestHelper>();
            _wageringConfigurationCommands = Container.Resolve<IWagerConfigurationCommands>();
            _avcConfigurationQueries = Container.Resolve<IAVCConfigurationQueries>();
            _avcConfigurationCommands = Container.Resolve<IAVCConfigurationCommands>();
            _brandQueries = Container.Resolve<BrandQueries>();

            Container.Resolve<SecurityTestHelper>().SignInUser();
            Container.Resolve<BonusWorker>().Start();
            Container.Resolve<RiskLevelWorker>().Start();

            var brandHelper = Container.Resolve<BrandTestHelper>();
            brandHelper.CreateActiveBrandWithProducts();


            var playerId = Container.Resolve<PlayerTestHelper>().CreatePlayer(null);
            var player = _playerQueries.GetPlayer(playerId);
            player.DateRegistered = DateTimeOffset.Now.AddMonths(-1);
            _paymentTestHelper.CreatePlayerBankAccount(playerId, true);

            _gameId = brandHelper.GetMainWalletGameId(playerId);

        }

        [Test]
        public void Can_not_create_ow_with_age_greater_then_setted_up()
        {
            //
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasAccountAge = true,
                                AccountAge = 4,
                                AccountAgeOperator = ComparisonEnum.LessOrEqual
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
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

            Assert.Throws<AccountAgeException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Can_create_ow_with_age_greater_then_setted_up()
        {
            //
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = false,
                                HasAccountAge = true,
                                AccountAge = 5,
                                AccountAgeOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
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

            var response =  _withdrawalService.Request(offlineWithdrawalRequest);

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public void Can_create_ow_with_deposit_count_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasDepositCount = true,
                                TotalDepositCountAmount = 1,
                                TotalDepositCountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
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

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public void Can_not_create_ow_with_deposit_count_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasDepositCount = true,
                                TotalDepositCountAmount = 6,
                                TotalDepositCountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 1000);
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

            Assert.Throws<DepositCountException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Can_create_ow_with_deposit_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = false,
                                HasTotalDepositAmount = true,
                                TotalDepositAmount = 500,
                                TotalDepositAmountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
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

            Assert.IsNotNull(response.Id);
        
        }


        [Test]
        public void Can_create_ow_with_total_withdrawal_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWithdrawalCount = true,
                                TotalWithdrawalCountAmount = 1,
                                TotalWithdrawalCountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentRepository.OfflineWithdraws.Add(new OfflineWithdraw()
            {
                Id = Guid.NewGuid(),
                Amount = 20,
                Status = WithdrawalStatus.Approved,
                PlayerBankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).First(x => x.Player.Id == player.Id)
            });
            _paymentRepository.OfflineWithdraws.Add(new OfflineWithdraw()
            {
                Id = Guid.NewGuid(),
                Amount = 20,
                Status = WithdrawalStatus.Approved,
                PlayerBankAccount = _paymentRepository.PlayerBankAccounts.Include(x => x.Player).First(x => x.Player.Id == player.Id)
            });

            _paymentRepository.SaveChanges();
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

            Assert.IsNotNull(response.Id);

        }

        [Test]
        public void Can_not_create_ow_with_total_withdrawal_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWithdrawalCount = true,
                                TotalWithdrawalCountAmount = 10,
                                TotalWithdrawalCountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
           
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

            Assert.Throws<WithdrawalCountException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Can_create_ow_with_winloss_rule_less_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinLoss = true,
                                WinLossAmount = 100,
                                WinLossOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 10, player.Id, _gameId);
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

            Assert.IsNotNull(response.Id);
        }

        [Test]
        public void Can_not_create_ow_with_winloss_rule_less_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinLoss = true,
                                WinLossAmount = 1000,
                                WinLossOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();

            _paymentTestHelper.MakeDeposit(player.Id, 1000);
            _gamesTestHelper.PlaceAndWinBet(1000, 10, player.Id, _gameId);
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

            Assert.Throws<WinLossException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test]
        public void Can_not_create_ow_with_deposit_amount_greater_than_setted_up()
        {
            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasTotalDepositAmount = true,
                                TotalDepositAmount = 10000,
                                TotalDepositAmountOperator = ComparisonEnum.Greater
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
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

            Assert.Throws<TotalDepositAmountException>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        [Test, Ignore("Until SergeyS fixes")]
        public void Can_not_make_ow_when_has_winning_amount_less_than_setted_up()
        {
            var gameId = Guid.Parse("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C"); // Football's game id
            var productId = Guid.Parse("18FB823B-435D-42DF-867E-3BA38ED92060"); // "Mock GameProvider" Poker game belongs to

            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = true,
                                WinningRules = new List<WinningRuleDTO>
                                {
                                    FraudTestDataHelper.GenerateWinningRule(productId)
                                },
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
            // var game = _winningRuleQueries.GetWinningRules();
            _paymentTestHelper.MakeDeposit(player.Id, 100);
            //TODO: Sergey Skripnik  - we are need to create Product and Games in BeforeAll and then get game.Id for making PlaceAndWinBet
            _gamesTestHelper.PlaceAndWinBet(100, 99, player.Id, gameId);

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
                Remarks = "rogi",
                RequestedBy = _securityProvider.User.UserName
            };

            Assert.Throws<HasWinningsAmountExeption>(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }


        [Test, Ignore("Until SergeyS fixes")]
        public void Can_make_ow_when_has_winning_amount_greater_than_setted_up()
        {
            //TODO: Sergey Skripnik  - removed hardcoded GameId 
            var gameId = Guid.Parse("BDCD4277-4FF7-46EF-B8ED-F4192E51F03C"); // Football's game id
            var productId = Guid.Parse("18FB823B-435D-42DF-867E-3BA38ED92060"); // "Mock GameProvider" Poker game belongs to

            var brand = _brandQueries.GetBrands().First();
            CreateAvcConfiguration(
                            new AVCConfigurationDTO
                            {
                                Brand = brand.Id,
                                Currency = brand.DefaultCurrency,
                                HasFraudRiskLevel = false,
                                HasWinnings = true,
                                WinningRules = new List<WinningRuleDTO>
                                {
                                    FraudTestDataHelper.GenerateWinningRule(productId)
                                },
                            }
                );

            // act 
            var player = _playerQueries.GetPlayers().ToList().First();
            _paymentTestHelper.MakeDeposit(player.Id, 101);

            _gameRepository.SavedChanges += (s, e) =>
            {
                var allGameActions = _gameRepository.Rounds
                    .SelectMany(o => o.GameActions);

                foreach (var gameAction in allGameActions)
                {
                    if (!Enumerable.Any(_gameRepository.GameActions, o => o.Id == gameAction.Id))
                    {
                        gameAction.Round.Game = _gameRepository.Games
                            .Single(o => o.Id == gameAction.Round.GameId);

                        _gameRepository.GameActions.Add(gameAction);
                    }
                }
            };
            
            _gamesTestHelper.PlaceAndWinBet(101, 101, player.Id, gameId);

            var offlineWithdrawalRequest = new OfflineWithdrawRequest
            {
                Amount = 1,
                NotificationType = NotificationType.None,
                BankAccountTime = _paymentRepository.PlayerBankAccounts.First().Created.ToString(),
                BankTime = _paymentRepository.Banks.First().Created.ToString(),
                PlayerBankAccountId = _paymentRepository
                   .PlayerBankAccounts.
                   Include(x => x.Player).
                   First(x => x.Player.Id == player.Id)
                   .Id,
                Remarks = "rogi",
                RequestedBy = _securityProvider.User.UserName
            };

            Assert.DoesNotThrow(() => _withdrawalService.Request(offlineWithdrawalRequest));
        }

        private void CreateAvcConfiguration(AVCConfigurationDTO avcConfiguration)
        {
            Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            avcConfiguration.Id = Guid.NewGuid();
            _avcConfigurationCommands.Create(avcConfiguration);
        }
    }
}
