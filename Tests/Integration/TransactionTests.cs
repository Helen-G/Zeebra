using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Bonus;
using AFT.RegoV2.Infrastructure.DataAccess.Brand;
using AFT.RegoV2.Infrastructure.DataAccess.Content;
using AFT.RegoV2.Infrastructure.DataAccess.Event;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Infrastructure.DataAccess.Player;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Repository;
using AFT.RegoV2.Infrastructure.DataAccess.Report;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using AutoMapper;
using FluentAssertions;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class TransactionTests : WebServiceTestsBase
    {
        private EventPublisherWorker _eventPublisherWorker;
        private BonusWorker _worker;
        private bool _workerIsRunning;
        private BonusManagementCommands _bonusCommands;
        private IPaymentRepository _paymentRepository;

        public override void BeforeAll()
        {
            base.BeforeAll();

            Mapper.CreateMap<RegisterRequest, RegistrationData>();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            Container.RegisterInstance(new Mock<IFileStorage>().Object);
            Container.RegisterType<IBonusRepository, BonusRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IPaymentRepository, PaymentRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IBrandRepository, BrandRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IPlayerRepository, PlayerRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IGameRepository, GameRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IContentRepository, ContentRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IReportRepository, ReportRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<IEventRepository, EventRepository>(new PerResolveLifetimeManager());
            Container.RegisterType<ISecurityRepository, SecurityRepository>(new PerResolveLifetimeManager());

            Container.Resolve<SecurityTestHelper>().SignInUser();

            if (_workerIsRunning)
            {
                return;
            }
            _eventPublisherWorker = Container.Resolve<EventPublisherWorker>();
            _eventPublisherWorker.Start();
            _worker = Container.Resolve<BonusWorker>();
            _worker.Start();
            Thread.Sleep(500);
            _workerIsRunning = true;
            _bonusCommands = Container.Resolve<BonusManagementCommands>();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
        }

        public override void AfterAll()
        {
            base.AfterAll();
            if ( _worker != null)
                _worker.Stop();
            if (_eventPublisherWorker != null)
                _eventPublisherWorker.Stop();
        }

        [Test, Explicit]
        public void Multiple_Players_betting_and_depositing_simultaneously()
        {
            const int playersInParallel = 5;
            const int timesToBet = 10;

            var bankAccountId = _paymentRepository.BankAccounts.First(a => a.Status == BankAccountStatus.Active).Id;
            var bonus = CreateBonus();

            var scenario = new Action(() =>
            {
                var playerId = CreatePlayer();
                Trace.WriteLine(string.Format("Player: {0} deposits in thread #{1}", playerId, Thread.CurrentThread.ManagedThreadId));

                const int depositAmount = timesToBet * 20;
                MakeDeposit(playerId, bonus.Code, depositAmount, bankAccountId);

                var gameId = Guid.NewGuid();
                var helper = Container.Resolve<GamesTestHelper>();
                for (var i = 0; i < timesToBet; i++)
                {
                    Trace.WriteLine(string.Format("Betting #{0} in thread #{1}", i, Thread.CurrentThread.ManagedThreadId));
                    helper.PlaceAndLoseBet(10, playerId, gameId);
                    helper.PlaceAndWinBet(10, 20, playerId, gameId);
                }
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = playersInParallel };
            Parallel.Invoke(options, Enumerable.Repeat(scenario, playersInParallel).ToArray());

            var bonusRepository = Container.Resolve<IBonusRepository>();
            bonus = bonusRepository
                .Bonuses
                .Include(b => b.Statistic)
                .Single(b => b.Id == bonus.Id);

            bonus.Statistic.TotalRedemptionCount.Should().Be(playersInParallel);
            bonus.Statistic.TotalRedeemedAmount.Should().Be(playersInParallel * 25);
        }

        [Test, Explicit]
        public void One_player_deposits_and_bets_simulteneously()
        {
            const decimal testAmount = 10m;
            const int simulationCount = 100;

            var playerId = CreatePlayer();
            var bankAccountId = _paymentRepository.BankAccounts.First(a => a.Status == BankAccountStatus.Active).Id;
            //create initial balance
            MakeDeposit(playerId, null, simulationCount * testAmount, bankAccountId);
            var bonus = CreateBonus();

            var depositing = new Action(() =>
            {
                for (int i = 0; i < simulationCount; i++)
                {
                    Console.WriteLine("Depositing #{0} in thread #{1}", i, Thread.CurrentThread.ManagedThreadId);
                    MakeDeposit(playerId, bonus.Code, testAmount, bankAccountId);
                }
            });

            var betting = new Action(() =>
            {
                var gameId = Guid.NewGuid();
                for (int i = 0; i < simulationCount; i++)
                {
                    Console.WriteLine("Betting #{0} in thread #{1}", i, Thread.CurrentThread.ManagedThreadId);
                    var helper = Container.Resolve<GamesTestHelper>();
                    helper.PlaceAndLoseBet(testAmount, playerId, gameId);
                }
            });

            Parallel.Invoke(depositing, betting);

            var walletRepository = Container.Resolve<GameRepository>();
            var wallet = walletRepository.Wallets.Single(w => w.PlayerId == playerId);
            wallet.Main.Should().Be(simulationCount * testAmount);
        }

        [Test, Explicit]
        public void One_player_bets_in_multiple_windows_simulteneously()
        {
            const decimal testAmount = 10m;
            const int simulationCount = 100;
            const int threadsCount = 2;

            var playerId = CreatePlayer();
            var bankAccountId = _paymentRepository.BankAccounts.First(a => a.Status == BankAccountStatus.Active).Id;
            //create initial balance
            MakeDeposit(playerId, null, simulationCount * testAmount * threadsCount, bankAccountId);

            var betting = new Action(() =>
            {
                var gameId = Guid.NewGuid();
                for (int i = 0; i < simulationCount; i++)
                {
                    Console.WriteLine("Betting #{0} in thread #{1}", i, Thread.CurrentThread.ManagedThreadId);
                    var helper = Container.Resolve<GamesTestHelper>();
                    helper.PlaceAndLoseBet(testAmount, playerId, gameId);
                }
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = threadsCount };
            Parallel.Invoke(options, Enumerable.Repeat(betting, threadsCount).ToArray());

            var walletRepository = Container.Resolve<GameRepository>();
            var wallet = walletRepository.Wallets.Single(w => w.PlayerId == playerId);
            wallet.Main.Should().Be(0);
        }

        private Guid CreatePlayer()
        {
            var playerRegData = TestDataGenerator.CreateRandomRegistrationRequestData();
            var registrationData = Mapper.DynamicMap<RegistrationData>(playerRegData);
            registrationData.CurrencyCode = "CAD";

            var playerCommands = Container.Resolve<PlayerCommands>();
            var playerId = playerCommands.Register(registrationData);
            WaitForPlayerRegistered(playerId, TimeSpan.FromSeconds(20));

            return playerId;
        }

        private void WaitForPlayerRegistered(Guid playerId, TimeSpan timeout)
        {
            var bonusRepository = Container.Resolve<IBonusRepository>();
            var stopwatch = Stopwatch.StartNew();
            while (bonusRepository.Players.All(p => p.Id != playerId) && stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(100);
            }
            if (bonusRepository.Players.All(p => p.Id != playerId))
            {
                throw new RegoException("Player registration timeout. " + stopwatch.Elapsed);
            }
            Trace.WriteLine(string.Format("Player: {0} created in thread #{1} on {2}", playerId, Thread.CurrentThread.ManagedThreadId, stopwatch.Elapsed));
        }

        private void MakeDeposit(Guid playerId, string bonusCode, decimal depositAmount, Guid bankAccountId)
        {
            var request = new OfflineDepositRequest
            {
                PlayerId = playerId,
                BonusCode = bonusCode,
                Amount = depositAmount,
                BankAccountId = bankAccountId
            };

            //should be 'per resolve' lifetime strategy
            var offlineDepositCommands = Container.Resolve<OfflineDepositCommands>();
            var offlineDeposit = offlineDepositCommands.Submit(request);

            var depositConfirm = new OfflineDepositConfirm
            {
                Id = offlineDeposit.Id,
                Amount = depositAmount
            };

            offlineDepositCommands.Confirm(depositConfirm, new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, null);
            offlineDepositCommands.Verify(offlineDeposit.Id, TestDataGenerator.GetRandomString());

            var depositApprove = new OfflineDepositApprove
            {
                Id = offlineDeposit.Id,
                ActualAmount = depositAmount,
                Remark = TestDataGenerator.GetRandomString()
            };
            offlineDepositCommands.Approve(depositApprove);
        }

        private Template CreateTemplate()
        {
            var bonusBuilder = Container.Resolve<BonusBuilder>();
            var templateUiData = new TemplateVM
            {
                Info = new TemplateInfoVM
                {
                    Name = TestDataGenerator.GetRandomString(),
                    BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                    WalletTemplateId = new Guid("9D366EF4-7AEF-4DFE-80E1-045909DC8EFD")
                }
            };
            var template = bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusCommands.AddUpdateTemplate(template);

            templateUiData = new TemplateVM
            {
                Id = template.Id,
                Availability = new TemplateAvailabilityVM(),
                Rules = new TemplateRulesVM
                {
                    RewardTiers = new List<RewardTierVM>
                    {
                        new RewardTierVM
                        {
                            CurrencyCode = "CAD",
                            BonusTiers = new List<TemplateTierVM>
                            {
                                new TemplateTierVM
                                {
                                    Reward = 25
                                }
                            }
                        }
                    },
                    RewardType = BonusRewardType.Amount,
                },
                Wagering = new TemplateWageringVM(),
                Notification = new TemplateNotification()
            };
            template = bonusBuilder.BuildTemplate(templateUiData).Entity;
            _bonusCommands.AddUpdateTemplate(template);

            return template;
        }

        private Bonus CreateBonus()
        {
            var bonusBuilder = Container.Resolve<BonusBuilder>();
            var template = CreateTemplate();
            var bonusUiData = new BonusVM
            {
                Name = TestDataGenerator.GetRandomString(5),
                Code = TestDataGenerator.GetRandomString(5),
                IsActive = true,
                TemplateId = template.Id,
                TemplateVersion = template.Version,
                ActiveFrom = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).ToString("d"),
                ActiveTo = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId).AddDays(1).ToString("d")
            };
            var bonus = bonusBuilder.BuildBonus(bonusUiData).Entity;
            _bonusCommands.AddBonus(bonus);

            return bonus;
        }
    }
}