using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Security.Providers;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class PlayerBankAccountTests : AdminWebsiteUnitTestsBase
    {
        private PlayerBankAccountCommands _playerBankAccountCommands;
        private IPaymentRepository _paymentRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var securityProvider = Container.Resolve<SecurityProvider>();
            securityProvider.SetUser(new User { Username = "test" });
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            var paymentQueries = Container.Resolve<IPaymentQueries>();
            var bus = Container.Resolve<IEventBus>();
            _playerBankAccountCommands = new PlayerBankAccountCommands(_paymentRepository, paymentQueries, securityProvider, bus);

            _paymentRepository.Banks.Add(new Bank {Id = Guid.NewGuid()});
            _paymentRepository.Players.Add(new Core.Payment.Data.Player {Id = Guid.NewGuid()});
            _paymentRepository.SaveChanges();
        }

        [Test]
        public void Can_create_player_bank_account()
        {
            var data = CreatePlayerBankAccountData();
            var player = _paymentRepository.Players.First();
            data.PlayerId = player.Id;
            var bank = _paymentRepository.Banks.First();
            data.Bank = bank.Id;

            _playerBankAccountCommands.Add(data);

            var playerBankAccount = _paymentRepository.PlayerBankAccounts.First();

            Assert.That(playerBankAccount, Is.Not.Null);
            Assert.That(playerBankAccount.AccountNumber, Is.EqualTo(data.AccountNumber));
            Assert.That(playerBankAccount.AccountName, Is.EqualTo(data.AccountName));
        }

        [Test]
        public void Can_edit_player_bank_account()
        {
            var data = CreatePlayerBankAccountData();
            var player = _paymentRepository.Players.First();
            data.PlayerId = player.Id;
            var bank = _paymentRepository.Banks.First();
            data.Bank = bank.Id;

            _playerBankAccountCommands.Add(data);

            var playerBankAccount = _paymentRepository.PlayerBankAccounts.First();

            Assert.That(playerBankAccount, Is.Not.Null);
            Assert.That(playerBankAccount.AccountNumber, Is.EqualTo(data.AccountNumber));

            var newAccountName = TestDataGenerator.GetRandomString();
            data.AccountName = newAccountName;
            data.Id = playerBankAccount.Id;

            _playerBankAccountCommands.Edit(data);

            playerBankAccount = _paymentRepository.PlayerBankAccounts.First();

            Assert.That(playerBankAccount, Is.Not.Null);
            Assert.That(playerBankAccount.AccountName, Is.EqualTo(newAccountName));
        }

        [Test]
        public void Can_set_current_player_bank_account()
        {
            var data = CreatePlayerBankAccountData();
            var player = _paymentRepository.Players.First();
            data.PlayerId = player.Id;
            var bank = _paymentRepository.Banks.First();
            data.Bank = bank.Id;

            _playerBankAccountCommands.Add(data);

            var newData = CreatePlayerBankAccountData();
            newData.PlayerId = player.Id;
            newData.Bank = bank.Id;

            _playerBankAccountCommands.Add(newData);

            var newPlayerBankAccount = _paymentRepository.PlayerBankAccounts.Single(x => x.AccountName == newData.AccountName);

            _playerBankAccountCommands.SetCurrent(newPlayerBankAccount.Id);

            newPlayerBankAccount = _paymentRepository.PlayerBankAccounts.Single(x => x.AccountName == newData.AccountName);

            Assert.That(newPlayerBankAccount.IsCurrent, Is.True);
        }

        public EditPlayerBankAccountCommand CreatePlayerBankAccountData()
        {
            var data = new EditPlayerBankAccountCommand
            {
                PlayerId = new Guid("66AF937B-63D3-4CD4-89C4-7EECAD4FBB05"),
                Bank = new Guid("46C5D75E-4DF4-41B0-AA71-1D5E8CDAA897"),
                AccountName = TestDataGenerator.GetRandomString(),
                AccountNumber = TestDataGenerator.GetRandomString(7, "0123456789"),
                Province = TestDataGenerator.GetRandomString(),
                City = TestDataGenerator.GetRandomString(),
                Branch = TestDataGenerator.GetRandomString(),
                SwiftCode = TestDataGenerator.GetRandomString(),
                Address = TestDataGenerator.GetRandomString(),
            };

            return data;
        }
    }
}