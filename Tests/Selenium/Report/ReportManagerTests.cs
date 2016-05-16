using System;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using AFT.RegoV2.Tests.Common.Helpers;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Until waiting for asyncronous records creating is implemented - AFTREGO-2760 - Maxim ")]
    class ReportManagerTests : SeleniumBaseForAdminWebsite
    {
        private DashboardPage _dashboardPage;
        private const string Licensee = "Flycow";
        private const string Brand = "138";

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
        }

        #region Player Reports
        [Test]
        public void Can_view_Player_Bet_History_report()
        {
            const decimal depositAmount = 100;
            // login to admin website, create a player and make an offline deposit
            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(Licensee, Brand);
            
            // login to member website and place a bet
            _container.Resolve<PaymentTestHelper>().MakeDeposit(playerData.LoginName, depositAmount);
            var playerProfilePage = _driver.LoginToMemberWebsite(playerData.LoginName, playerData.Password);
            var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            var gamePage = gameListPage.StartGame("Roulette");
            gamePage.PlaceInitialBet(depositAmount, "placed bet");
            
            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerReportsPage = _dashboardPage.Menu.ClickPlayerReportsMenuItem();
            var reportTab = playerReportsPage.OpenPlayerBetHistoryReport();
            
            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab, "Username");

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "Username", condition: "is", value: playerData.LoginName);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        [Test]
        public void Can_view_Player_report()
        {
            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(Licensee, Brand,"CNY");
            
            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerReportsPage = _dashboardPage.Menu.ClickPlayerReportsMenuItem();
            var reportTab = playerReportsPage.OpenPlayerReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab);

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "Username", condition: "is", value: playerData.LoginName);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        #endregion
        #region Payment Reports
        [Test]
        public void Can_view_Deposit_report()
        {
            const decimal depositAmount = 150;

            // login to admin website, create a player and make an offline deposit
            var playerData = _driver.LoginAsSuperAdminAndCreatePlayer(Licensee, Brand, "CNY");
            var menu = new BackendMenuBar(_driver);
            var playerManagerPage = menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var newOfflineDepositRequest = playerManagerPage.OpenOfflineDepositRequestForm();
            var submittedOfflineDepositRequestForm = newOfflineDepositRequest.Submit(amount: depositAmount);
            var referenceCode = submittedOfflineDepositRequestForm.ReferenceCode;

            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var paymentReportsPage = _dashboardPage.Menu.ClickPaymentReportsMenuItem();
            var reportTab = paymentReportsPage.OpenDepositReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab, "Transaction ID");

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "Transaction ID", condition: "is", value: referenceCode);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        #endregion
        #region Brand Reports
        [Test]
        public void Can_view_Brand_report()
        {
            // login to admin website, create a brand
            var randomString = TestDataGenerator.GetRandomString(4);
            var brandName = "brand-" + randomString;
            var brandCode = randomString;
            var playerPrefix = TestDataGenerator.GetRandomAlphabeticString(3);
            const string brandType = "Credit";

            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var brandManagerPage = _dashboardPage.Menu.ClickBrandManagerItem();

            // create a brand
            var newBrandForm = brandManagerPage.OpenNewBrandForm();
            var submittedBrandForm = newBrandForm.Submit(brandName, brandCode, playerPrefix, brandType);
            _dashboardPage.BrandFilter.SelectAll();
            
            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var paymentReportsPage = _dashboardPage.Menu.ClickBrandReportsMenuItem();
            var reportTab = paymentReportsPage.OpenBrandReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab);

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "Brand Name", condition: "is", value: brandName);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        [Test]
        public void Can_view_Licensee_report()
        {
            // login to admin website
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            
            //create a licensee
            var licenseeName = "Licensee-" + TestDataGenerator.GetRandomString(5);
            var companyName = "Company-" + TestDataGenerator.GetRandomString(5);
            var contractStartDate = DateTime.Now.ToString("yyyy'/'MM'/'dd");
            var contractEndDate = DateTime.Now.AddMonths(5).ToString("yyyy'/'MM'/'dd");
            var email = TestDataGenerator.GetRandomEmail();
            
            var licenseeManagerPage = _dashboardPage.Menu.ClickLicenseeManagerItem();
            var newLicenseeForm = licenseeManagerPage.OpenNewLicenseeForm();
            newLicenseeForm.Submit(licenseeName, companyName, contractStartDate, contractEndDate, "5", "6", email);
            _dashboardPage.BrandFilter.SelectAll();

            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var paymentReportsPage = _dashboardPage.Menu.ClickBrandReportsMenuItem();
            var reportTab = paymentReportsPage.OpenLicenseeReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab);

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "Licensee Name", condition: "is", value: licenseeName);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        [Test]
        public void Can_view_Language_report()
        {
            // login to admin website
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();

            // create language
            var languages = _dashboardPage.Menu.ClickLanguageManagerMenuItem();
            var newLanguageForm = languages.OpenNewLanguageForm();
            var code = TestDataGenerator.GetRandomAlphabeticString(3);
            var name = "Name" + code;
            var nativeName = "NativeName" + code;
            var submittedLanguageForm = newLanguageForm.Submit(code, name, nativeName);

            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var paymentReportsPage = _dashboardPage.Menu.ClickBrandReportsMenuItem();
            var reportTab = paymentReportsPage.OpenLanguageReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab);

            // generate report with existed (just created) data, check that report contains at least one record
            _driver.TypeFilterCriterion(column: "Language Code", condition: "is", value: code);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        [Test]
        public void Can_view_VIP_Level_report()
        {
            // create VIP level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(Licensee, Brand, defaultForNewPlayers: false);

            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            
            var newForm = vipLevelsPage.OpenNewVipLevelForm();
            newForm.Submit(vipLevelData);

            // open report page
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var paymentReportsPage = _dashboardPage.Menu.ClickBrandReportsMenuItem();
            var reportTab = paymentReportsPage.OpenVipLevelReport();

            // generate empty report in order to ensure, that grid contains no records before actual filter is applied
            GenerateEmptyReport(reportTab);

            // generate report with existed (just created) data, check that report contains one record
            _driver.TypeFilterCriterion(column: "VIP Level", condition: "is", value: vipLevelData.Code);
            _driver.GenerateReport();

            var reportDataDisplayed = reportTab.CheckIfAnyReportDataDisplayed();
            Assert.IsTrue(reportDataDisplayed);
        }

        #endregion
        #region Common
        private void GenerateEmptyReport(ReportPageBase reportTab, string fieldName = "Created By")
        {
            // generate report, filtering it by unexisted (random) data, so that report should be empty
            _driver.TypeFilterCriterion(column: fieldName, condition: "is", value: TestDataGenerator.GetRandomString());
            _driver.GenerateReport();
            reportTab.WaitUntilReportDataIsEmpty();
        }

        #endregion
    }
}
