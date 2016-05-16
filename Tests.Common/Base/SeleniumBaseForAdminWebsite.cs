using System;
using System.Web.Configuration;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class SeleniumBaseForAdminWebsite : SeleniumBase
    {
        protected override string GetWebsiteUrl()
        {
            return WebConfigurationManager.AppSettings["AdminWebsiteUrl"];
        }

        protected SubmittedOfflineWithdrawRequestForm SubmitOfflineWithdrawalRequest(PlayerRegistrationDataForAdminWebsite playerData, PlayerBankAccountData playerBankAccountData, OfflineWithdrawRequestData withdrawalData)
        {
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var playerInfoPage = playerManagerPage.OpenPlayerInfoPage();
            playerInfoPage.OpenBankAccountsSection();
            var playerBankAccountForm = playerInfoPage.OpenNewBankAccountTab();
            playerBankAccountForm.Submit(playerBankAccountData);
            playerManagerPage.CloseTab("View Bank Account");
            playerManagerPage.CloseTab("Player Info");
            var playerBankAccountVerifyPage = playerManagerPage.Menu.ClickPlayerBankAccountVerifyMenuItem();
            playerBankAccountVerifyPage.Verify(playerBankAccountData.BankAccountName);
            dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            playerManagerPage = dashboardPage.Menu.ClickPlayerManagerMenuItem();
            playerManagerPage.SelectPlayer(playerData.LoginName);
            var withdrawRequestForm = playerManagerPage.OpenOfflineWithdrawRequestForm(playerData.LoginName);
            return withdrawRequestForm.Submit(withdrawalData);
        }
    }
}