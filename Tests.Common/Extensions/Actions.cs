using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Extensions
{
    public static class Actions
    {
        public static DashboardPage DashboardPage;

        public static DashboardPage LoginToAdminWebsiteAsSuperAdmin(this IWebDriver driver)
        {
            var cookie = driver.Manage().Cookies.GetCookieNamed(".ASPXAUTH");
            if (cookie == null)
                return driver.LoginToAdminWebsiteAs("SuperAdmin", "SuperAdmin");
            return DashboardPage;
        }

        public static void Logout(this IWebDriver driver)
        {
            driver.Manage().Cookies.DeleteAllCookies();
            driver.Navigate().Refresh();
        }

        public static PlayerProfilePage LoginToMemberWebsite(this IWebDriver driver, string username, string password)
        {
            var loginPage = new MemberWebsiteLoginPage(driver);
            loginPage.NavigateToMemberWebsite();
            Logout(driver);
            var playerProfilePage = loginPage.Login(username, password);
            return playerProfilePage;
        }

        public static DashboardPage LoginToAdminWebsiteAs(this IWebDriver driver, string login, string password)
        {
            var loginPage = new AdminWebsiteLoginPage(driver);
            loginPage.NavigateToAdminWebsite();
            Logout(driver);
            return loginPage.Login(login, password);
        }

        public static DashboardPage MakeOfflineDeposit(this IWebDriver driver, string username, decimal depositAmount, string fullName, string bonusName = null, string bonusCode = null)
        {
            driver.LoginToAdminWebsiteAsSuperAdmin();
            var menu = new BackendMenuBar(driver);
            var playerManagerPage = menu.ClickPlayerManagerMenuItem();

            //create a deposit request, apply a bonus
            playerManagerPage.SelectPlayer(username);
            var offlineDepositRequestForm = playerManagerPage.OpenOfflineDepositRequestForm();
            SubmittedOfflineDepositRequestForm submittedOfflineDeposit;
            if (bonusName == null && bonusCode == null)
            {
                submittedOfflineDeposit = offlineDepositRequestForm.Submit(depositAmount);
                Assert.AreEqual("Offline deposit request has been created successfully", submittedOfflineDeposit.Confirmation);
            }
            else
            {
                submittedOfflineDeposit = offlineDepositRequestForm.SubmitWithBonusCode(bonusCode, bonusName, depositAmount);
                Assert.AreEqual("Offline deposit request has been created successfully", submittedOfflineDeposit.Confirmation);
            }
            var referenceCode = submittedOfflineDeposit.ReferenceCode;

            //confirm a deposit request
            var offlineDepositRequestsPage = menu.ClickOfflineDepositConfirmMenuItem();
            offlineDepositRequestsPage.SelectOfflineDepositRequest(username, referenceCode);
            var depositConfirmPage = offlineDepositRequestsPage.ClickConfirmButton();
            var validDepositConfirmData = TestDataGenerator.CreateValidDepositConfirmData(fullName, depositAmount);
            var submittedConfirmDeposit = depositConfirmPage.SubmitValidDepositConfirm(validDepositConfirmData);
            Assert.AreEqual("Offline deposit request has been confirmed successfully", submittedConfirmDeposit.GetConfirmationMessage);

            //verify a deposit request
            var playerDepositVerifyPage = menu.ClickPlayerDepositVerifyItem();
            playerDepositVerifyPage.FilterGrid(username);
            playerDepositVerifyPage.SelectConfirmedDeposit(referenceCode);
            var verifyForm = playerDepositVerifyPage.OpenVerifyForm();
            var submittedVerifyDeposit = verifyForm.Submit();
            Assert.AreEqual("Offline deposit request has been verified successfully", submittedVerifyDeposit.ConfirmationMessage);

            //approve a deposit request
            var playerDepositApprovePage = menu.ClickPlayerDepositApproveItem();
            playerDepositApprovePage.FilterGrid(username);
            playerDepositApprovePage.SelectVerifiedDeposit(referenceCode);
            var approveForm = playerDepositApprovePage.OpenApproveForm();
            var submittedApproveDeposit = approveForm.Submit(depositAmount, "0");
            Assert.AreEqual("Offline deposit request has been approved successfully", submittedApproveDeposit.ConfirmationMessage);

            driver.Navigate().Refresh();
            var dashboardPage = new DashboardPage(driver);
            return dashboardPage;
        }

        public static SubmittedNewUserForm CreateUser(this IWebDriver driver, RoleData roleData, AdminUserRegistrationData userData, string[] permissions)
        {
            // create a role
            var menu = new BackendMenuBar(driver);
            var roleManagerPage = menu.ClickRoleManagerMenuItem();
            var newRoleForm = roleManagerPage.OpenNewRoleForm();
            newRoleForm.SelectPermissions(permissions);
            var submittedForm = newRoleForm.FillInRequiredFieldsAndSubmit(roleData);
            submittedForm.CloseTab("View Role");

            // create a user
            var adminManagerPage = submittedForm.Menu.ClickAdminManagerMenuItem();
            var newUserForm = adminManagerPage.OpenNewUserForm();
            newUserForm.Submit(userData);
            return new SubmittedNewUserForm(driver);
        }

        public static SubmittedNewUserForm CreateUserBasedOnPredefinedRole(this IWebDriver driver, AdminUserRegistrationData userData)
        {
            var menu = new BackendMenuBar(driver);
            var adminManagerPage = menu.ClickAdminManagerMenuItem();
            var newUserForm = adminManagerPage.OpenNewUserForm();
            newUserForm.Submit(userData);
            return new SubmittedNewUserForm(driver);
        }

        public static PlayerRegistrationDataForAdminWebsite LoginAsSuperAdminAndCreatePlayer(this IWebDriver driver,
            string licensee, string brand, string currency = null, string culture = null, string country = null, string status = "Active")
        {
            driver.LoginToAdminWebsiteAsSuperAdmin();
            return driver.CreatePlayerAsAdmin(licensee, brand, currency, culture, country, status);
        }

        public static PlayerRegistrationDataForAdminWebsite CreatePlayerAsAdmin(this IWebDriver driver,
           string licensee, string brand, string currency = null, string culture = null, string country = null, string status = "Active")
        {
            var menu = new BackendMenuBar(driver);
            var playerManagerPage = menu.ClickPlayerManagerMenuItem();
            var newPlayerForm = playerManagerPage.OpenNewPlayerForm();
            var playerDataForAdminWebsite = TestDataGenerator.CreateValidPlayerDataForAdminWebsite(licensee, brand, currency, culture, country);
            playerDataForAdminWebsite.AccountStatus = status;
            var submittedPlayerForm = newPlayerForm.Register(playerDataForAdminWebsite);
            submittedPlayerForm.CloseTab("View Player");
            driver.Navigate().Refresh();

            return playerDataForAdminWebsite;
        }
        public static bool CheckIfButtonDisplayed(this IWebDriver driver, By buttonXPath)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try
            {
                wait.Until(d =>
                {
                    var foundElements = driver.FindElements(buttonXPath).FirstOrDefault(x => x.Displayed);
                    return foundElements != null;
                });
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

    }
}