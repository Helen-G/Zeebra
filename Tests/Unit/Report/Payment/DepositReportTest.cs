using System;
using System.Linq;
using System.Text;
using AFT.RegoV2.AdminWebsite.Controllers;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.BoundedContexts.Report;
using AFT.RegoV2.BoundedContexts.Report.Data;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.WinService.Workers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using PlayerData = AFT.RegoV2.Core.Player.Data.Player;

namespace AFT.RegoV2.Tests.Unit.Report.Payment
{
    internal class DepositReportTest : ReportsTestsBase
    {
        private IReportRepository _reportRepository;
        private ReportQueries _reportQueries;
        private ISecurityProvider _securityProvider;

        private PlayerData _player;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _reportRepository = Container.Resolve<IReportRepository>();
            _reportQueries = Container.Resolve<ReportQueries>();
            _securityProvider = Container.Resolve<ISecurityProvider>();

            _player = Container.Resolve<PlayerTestHelper>().CreatePlayer();
            
        }

        protected override void StartWorkers()
        {
            Container.Resolve<DepositReportWorker>().Start();
            Container.Resolve<SecurityTestHelper>().SignInUser(new Core.Security.Data.User() { Username = TestDataGenerator.GetRandomString() });
        }

        [Test]
        public void Can_process_submit_deposit()
        {
            // Arrange
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();

            // Act
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(CurrentBrand.Name, record.Brand);
            Assert.AreEqual(CurrentBrand.Licensee.Name, record.Licensee);
            Assert.AreEqual(_player.Username, record.Username);
            Assert.AreEqual(_player.InternalAccount, record.IsInternalAccount);
            Assert.AreEqual(deposit.TransactionNumber, record.TransactionId);
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.AreEqual("Offline-Bank", record.PaymentMethod);
            Assert.AreEqual(deposit.CurrencyCode, record.Currency);
            Assert.AreEqual(depositAmount, record.Amount);
            Assert.AreEqual(OfflineDepositStatus.New.ToString(), record.Status);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Submitted);
            Assert.AreEqual(SecurityTestHelper.CurrentUser.UserName, record.SubmittedBy);
            Assert.AreEqual(deposit.DepositType.ToString(), record.DepositType);
            Assert.AreEqual(deposit.BankAccount.AccountName, record.BankAccountName);
            Assert.AreEqual(deposit.BankAccount.AccountId, record.BankAccountId);
            Assert.AreEqual("Test Bank", record.BankName);
            Assert.AreEqual("Province", record.BankProvince);
            Assert.AreEqual("Branch", record.BankBranch);
            Assert.AreEqual(deposit.BankAccount.AccountNumber, record.BankAccountNumber);
        }

        [Test]
        public void Can_process_confirm_deposit()
        {
            // Arrange
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);

            // Act
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.AreEqual(depositAmount, record.Amount);
            Assert.AreEqual(OfflineDepositStatus.Processing.ToString(), record.Status);
        }

        [Test]
        public void Can_process_verify_deposit()
        {
            // Arrange
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);

            // Act
            PaymentTestHelper.VerifyOfflineDeposit(deposit, true);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Verified);
            Assert.AreEqual(_securityProvider.User.UserName, record.VerifiedBy);
            Assert.AreEqual(OfflineDepositStatus.Verified.ToString(), record.Status);
        }

        [Test]
        public void Can_process_unverify_deposit()
        {
            // Arrange
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);

            // Act
            PaymentTestHelper.VerifyOfflineDeposit(deposit, false);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Rejected);
            Assert.AreEqual(_securityProvider.User.UserName, record.RejectedBy);
            Assert.AreEqual(OfflineDepositStatus.Unverified.ToString(), record.Status);
        }

        [Test]
        public void Can_process_approve_deposit()
        {
            // Arrange
            const decimal fee = 3.5m;
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);
            PaymentTestHelper.VerifyOfflineDeposit(deposit, true);

            // Act
            PaymentTestHelper.ApproveOfflineDeposit(deposit, true, fee);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Approved);
            Assert.AreEqual(_securityProvider.User.UserName, record.ApprovedBy);
            Assert.AreEqual(fee, record.Fee);
            Assert.AreEqual(depositAmount - fee, record.ActualAmount);
            Assert.AreEqual(OfflineDepositStatus.Approved.ToString(), record.Status);
        }

        [Test]
        public void Can_process_reject_deposit()
        {
            // Arrange
            var depositAmount = TestDataGenerator.GetRandomDepositAmount();
            var deposit = PaymentTestHelper.CreateOfflineDeposit(_player.Id, depositAmount);
            PaymentTestHelper.ConfirmOfflineDeposit(deposit);
            PaymentTestHelper.VerifyOfflineDeposit(deposit, true);

            // Act
            PaymentTestHelper.ApproveOfflineDeposit(deposit, false);

            // Assert
            Assert.AreEqual(1, _reportRepository.DepositRecords.Count());
            var record = _reportRepository.DepositRecords.Single();
            Assert.AreEqual(deposit.Id, record.DepositId);
            Assert.Less(DateTimeOffset.Now.AddDays(-2), record.Rejected);
            Assert.AreEqual(_securityProvider.User.UserName, record.RejectedBy);
            Assert.AreEqual(OfflineDepositStatus.Rejected.ToString(), record.Status);
        }

        [Test]
        public void Can_export_report_data()
        {
            // Arrange
            PaymentTestHelper.CreateOfflineDeposit(_player.Id, TestDataGenerator.GetRandomDepositAmount());

            var filteredRecords = ReportController.FilterAndOrder(
                _reportQueries.GetDepositRecordsForExport(),
                new DepositRecord(),
                "Submitted", "asc");

            // Act
            var content = Encoding.Unicode.GetString(ReportController.ExportToExcel(filteredRecords));

            // Assert
            Assert.AreNotEqual(content.IndexOf("<table"), -1);
        }
    }
}