using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Report.Player
{
    internal class PlayerReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private ReportQueries _reportQueries;
        private PlayerCommands _playerCommands;
        private PlayerTestHelper PlayerTestHelper { get; set; }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _playerCommands = Container.Resolve<PlayerCommands>();

            PlayerTestHelper = Container.Resolve<PlayerTestHelper>();
        }

        protected override void StartWorkers()
        {
            Container.Resolve<PlayerReportWorker>().Start();
        }

        [Test]
        public void Can_process_create_player()
        {
            // Act
            var player = PlayerTestHelper.CreatePlayer();

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerRecords.Count());
            var record = _reportRepository.PlayerRecords.Single();
            Assert.AreEqual(CurrentBrand.Name,                      record.Brand);
            Assert.AreEqual(CurrentBrand.Licensee.Name,             record.Licensee);
            Assert.AreEqual(player.Username,                        record.Username);
            Assert.AreEqual(player.Email,                           record.Email);
            Assert.AreEqual(player.PhoneNumber,                     record.Mobile);
            Assert.AreEqual(player.InternalAccount,                 record.IsInternalAccount);
            Assert.Less(DateTimeOffset.Now.AddDays(-2),             record.RegistrationDate);
            Assert.AreEqual(player.IpAddress ?? LocalIPAddress,     record.SignUpIP);
            Assert.AreEqual(CurrentBrand.BrandCountries.First().Country.Name,    record.Country);
            var brandCulture = CurrentBrand.BrandCultures.Single(x => x.CultureCode == CurrentBrand.DefaultCulture);
            Assert.AreEqual(brandCulture.Culture.Name,              record.Language);
            Assert.AreEqual(CurrentBrand.DefaultCurrency,           record.Currency);
        }

        [Test]
        public void Can_process_update_player()
        {
            // Arrange
            const string newAddress = "#305-1250 Homer Street";
            const string newPhoneNumber = "16046287716";
            const string birthday = "1980/01/01";

            var player = PlayerTestHelper.CreatePlayer();
            var paymentLevel = Container.Resolve<PaymentTestHelper>().CreatePaymentLevel(CurrentBrand.Id, player.CurrencyCode);

            // Act
            _playerCommands.Edit(new EditPlayerData
            {
                PlayerId = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                MailingAddressCity = player.MailingAddressCity,
                MailingAddressPostalCode = player.MailingAddressPostalCode,
                CountryCode = player.CountryCode,
                Email = player.Email,
                DateOfBirth = birthday,
                MailingAddressLine1 = newAddress,
                PhoneNumber = newPhoneNumber,
                PaymentLevelId = paymentLevel.Id,
                Title = player.Title,
                Gender = player.Gender,
                AccountAlertEmail = true,
                MarketingAlertEmail = true
            });

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerRecords.Count());
            var record = _reportRepository.PlayerRecords.Single();
            Assert.AreEqual(player.Email,                               record.Email);
            Assert.AreEqual(newAddress,                                 record.StreetAddress);
            Assert.AreEqual(newPhoneNumber,                             record.Mobile);
            var expectedDateString = DateTimeOffset.Parse(birthday + " +0:00").ToString("yyyy'/'MM'/'dd");
            var actualDateString = record.Birthday.ToString("yyyy'/'MM'/'dd");
            Assert.AreEqual(expectedDateString, actualDateString);
        }

        [Test]
        public void Can_process_activate_player()
        {
            // Arrange
            var player = PlayerTestHelper.CreatePlayer(false);

            // Act
            _playerCommands.SetStatus(player.Id, true);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerRecords.Count());
            var record = _reportRepository.PlayerRecords.Single();
            Assert.AreEqual(AccountStatus.Active.ToString(), record.PlayerStatus);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Activated);
        }

        [Test]
        public void Can_process_deactivate_player()
        {
            // Arrange
            var player = PlayerTestHelper.CreatePlayer();

            // Act
            _playerCommands.SetStatus(player.Id, false);

            // Assert
            Assert.AreEqual(1, _reportRepository.PlayerRecords.Count());
            var record = _reportRepository.PlayerRecords.Single();
            Assert.AreEqual(AccountStatus.Inactive.ToString(), record.PlayerStatus);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Deactivated);
        }

        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            PlayerTestHelper.CreatePlayer();

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetPlayerRecordsForExport(),
                new PlayerRecord(),
                "RegistrationDate", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }
   }
}