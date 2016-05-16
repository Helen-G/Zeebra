using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.Tests.Unit.Wallet.Steps
{
    [Binding]
    public class DepositWithdrawSteps : WalletStepsBase
    {
        private readonly IEventRepository _eventRepository;

        public DepositWithdrawSteps(SpecFlowContainerFactory factory)
            : base(factory)
        {
            WalletRepository = Container.Resolve<IGameRepository>();
            WalletCommands = Container.Resolve<WalletCommands>();
            _eventRepository = Container.Resolve<IEventRepository>();
        }

        [Given(@"a wallet with no transactions")]
        public void GivenAWalletWithNoTransactions()
        {
            CreateWallet(true);
        }

        [Given(@"a player wallet \#(.*) with no transactions")]
        public void GivenAPlayerWalletWithNoTransactions(int walletIndex)
        {
            CreateWallet(walletIndex == 0);
        }

        [Given(@"I deposited \$(.*)")]
        [When(@"I deposited \$(.*)")]
        public void GivenIDeposited(decimal depositAmount)
        {
            if (!WalletRepository.Wallets.Any())
                GivenAWalletWithNoTransactions();
            WrapCallInExceptionCatch(() => WalletCommands.Deposit(Wallet.PlayerId, depositAmount, ""));
        }

        [Then(@"Main balance should be \$(.*)")]
        public void ThenMainBalanceShouldBe(decimal mainBalance)
        {
            ThenWalletMainBalanceShouldBe(1, mainBalance);
        }

        [Then(@"wallet \#(.*) Main balance should be \$(.*)")]
        public void ThenWalletMainBalanceShouldBe(int walletIndex, decimal mainBalance)
        {
            var wallet = Wallets.ElementAt(walletIndex - 1);
            wallet.Main.Should().Be(mainBalance);
        }

        [When(@"I withdraw \$(.*)")]
        [Given(@"I withdraw \$(.*)")]
        public void WhenIWithdraw(decimal amount)
        {
            WrapCallInExceptionCatch(() => WalletCommands.Withdraw(Wallet.PlayerId, amount, "000000"));
        }

        [Then(@"(.*) (.*) events should be sent")]
        public void ThenTransactionProcessedEventsShouldBeSent(int expectedEventsCount, string eventName)
        {
            _eventRepository.Events.Count(e => e.DataType == eventName).Should().Be(expectedEventsCount);
        }

        private void CreateWallet(bool isMain)
        {
            var templateId = Guid.NewGuid();
            var gameProviderId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var brandId = Guid.NewGuid();

            if (WalletRepository.Players.Any() == false)
            {
                WalletRepository.Players.Add(new Core.Game.Data.Player
                {
                    Id = playerId,
                    BrandId = brandId
                });
            }
            else
            {
                var player = WalletRepository.Players.First();
                playerId = player.Id;
                brandId = player.BrandId;
            }

            WalletRepository.GameProviders.Add(new GameProvider
            {
                Id = gameProviderId
            });

            WalletRepository.Games.Add(new Core.Game.Data.Game
            {
                Id = Guid.NewGuid(),
                GameProviderId = gameProviderId
            });

            var template = new WalletTemplate
            {
                Id = templateId,
                BrandId = brandId,
                IsMain = isMain,
                WalletTemplateGameProviders = new List<WalletTemplateGameProvider>()
                {
                    new WalletTemplateGameProvider
                    {
                        Id = Guid.NewGuid(),
                        WalletTemplateId = templateId,
                        GameProviderId = gameProviderId
                    }
                }
            };

            WalletRepository.WalletTemplates.Add(template);

            WalletRepository.Wallets.Add(new Core.Game.Data.Wallet
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Template = template,
                Brand = new Core.Game.Data.Brand
                {
                    TimezoneId = TestDataGenerator.GetRandomTimeZone().Id
                }
            });

        }
    }
}