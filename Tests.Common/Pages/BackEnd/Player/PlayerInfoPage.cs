using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerInfoPage : BackendPageBase
    {
        public PlayerInfoPage(IWebDriver driver) : base(driver) { }

        public string PageXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]/div/div[@data-view='player-manager/info']";

        public static By EditButton = By.XPath("//button[text()='Edit']");
        public static By DeactivateButton = By.XPath("//span[@data-bind='text: activateButtonText']");
        public static By ActivateButton = By.XPath("//button[text()='Activate']");
        public static By SaveButton = By.XPath("//button[text()='Save']");

        public string VipLevel
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                var vipLevel = _driver.FindElementWait(By.XPath("//p[contains(@data-bind, 'text: vipLevelName')]"));
                wait.Until(x => vipLevel.Displayed);
                return vipLevel.Text;
            }
        }

        public string Username
        {
            get { return _driver.FindElementValue(By.XPath("//p[@data-bind='text: username']")); }
        }

        public string FirstName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'text: firstName')]"));
            }
        }

        public string LastName
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath("//p[contains(@data-bind, 'text: lastName')]"));
            }
        }

        public void OpenTransactionsSection()
        {
            var transactionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'transactions')]"));
            transactionsSection.Click();
        }

        public void OpenAccountInformationSection()
        {
            var transactionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'account-information')]"));
            transactionsSection.Click();
        }

        public void OpenBalanceInformationSection()
        {
            //Thread.Sleep(5000);
            var balanceSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'balance-information')]"));
            balanceSection.Click();
        }

        public void OpenWithdrawHistorySection()
        {
            var withdrawHistorySection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'withdraw-history')]"));
            withdrawHistorySection.Click();
        }

        public void OpenAccountRestrictionsSection()
        {
            var accountRestrictionsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'account-restrictions')]"));
            accountRestrictionsSection.Click();
        }

        public void OpenBankAccountsSection()
        {
            var bankAccountsSection = _driver.FindElementWait(By.XPath("//a[contains(@id, 'bank-accounts')]"));
            bankAccountsSection.Click();
        }

        public decimal GetTransactionMainAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var stringValue = _driver.FindElementValue(By.XPath(xpath));
            return decimal.Parse(stringValue);
        }

        public decimal GetTransactionBonusAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var stringValue = _driver.FindElementValue(By.XPath(xpath));
            return decimal.Parse(stringValue);
        }

        public decimal[] GetTransactionsMainAmount(string type)
        {
            var xpath = string.Format("//td[text()='{0}']/following-sibling::td[contains(@aria-describedby, 'Amount')]", type);
            var elements = _driver.FindAnyElementsWait(By.XPath(xpath), element => element.Text != string.Empty);
            return elements.Select(webElement => decimal.Parse(webElement.Text)).ToArray();
        }

        public string GetMainBalance()
        {
            var xpath = string.Format("//p[@data-bind='text: balance().mainBalance']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetBonusBalance()
        {
            var xpath = string.Format("//p[@data-bind='text: balance().bonusBalance']");
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetWithdrawTransactionAmount(string amount)
        {
            var xpath = string.Format("//table[contains(@id, 'withdraw-list')]//td[contains(@title, '{0}')]", amount);
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public string GetWithdrawTransactionStatus(string status)
        {
            var xpath = string.Format("//table[contains(@id, 'withdraw-list')]//td[contains(@title, '{0}')]", status);
            return _driver.FindElementValue(By.XPath(xpath));
        }

        public void DeactivatePlayer()
        {
            var deactivateButton = _driver.FindElementWait(DeactivateButton);
            deactivateButton.Click();
        }

        public void ActivatePlayer()
        {
            //var section = _driver.FindElementWait(By.XPath("//a[contains(@href,'#account-info')]"));
            //section.Click();
            var activateButton = _driver.FindElementWait(By.XPath("//button/span[text()='Activate']"));
            activateButton.Click();
        }

        public AccountInformation GetAccountDetails()
        {
            return new AccountInformation
            {
                VIPLevel = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: vipLevelName')]")),
                PaymentLevel = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: paymentLevelName')]")),
            };
        }

        public void OpenAccountDetailsInEditMode()
        {
            OpenAccountInformationSection();
            var editButtonButton = _driver.FindElementWait(EditButton);
            editButtonButton.Click();
        }

        public ChangeVipLevelDialog OpenChangeVipLevelDialog()
        {
            var changeVipLevelDialogButton =
                _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: showChangeVipLevelDialog')]"));
            changeVipLevelDialogButton.Click();

            return new ChangeVipLevelDialog(_driver);
        }

        public void AssignVipLevel(string vipLevel)
        {
            var xpath = _driver.FindElementWait(By.XPath("//select[contains(@id, 'vip-level')]"));
            var vipLevelList = new SelectElement(xpath);
            vipLevelList.SelectByText(vipLevel);
        }

        public void SaveAccountInfo()
        {
            var saveButton = _driver.FindElementWait(SaveButton);
            saveButton.Click();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var editButton = _driver.FindElementWait(EditButton);
            wait.Until(x => editButton.Enabled);
        }

        public void Edit(PlayerRegistrationDataForAdminWebsite editedPlayerData)
        {
            var firstName =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: firstName')]"));
            firstName.Clear();
            firstName.SendKeys(editedPlayerData.FirstName);

            var lastName =
                _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: lastName')]"));
            lastName.Clear();
            lastName.SendKeys(editedPlayerData.LastName);

            var email = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: email')]"));
            email.Clear();
            email.SendKeys(editedPlayerData.Email);

            var mobileNumber = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: phoneNumber')]"));
            mobileNumber.Clear();
            mobileNumber.SendKeys(editedPlayerData.MobileNumber);

            var country = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: countries')]"));
            country.SendKeys(editedPlayerData.Country);

            SaveAccountInfo();
        }

        public SendNewPlayerPasswordDialog OpenSendNewPasswordDialog()
        {
            var sendNewPasswordButton = _driver.FindElementWait(By.XPath("//button[@data-bind='click: showSendMessageDialog']"));
            sendNewPasswordButton.Click();
            var dialog = new SendNewPlayerPasswordDialog(_driver);
            return dialog;
        }

        public PlayerBankAccountForm OpenNewBankAccountTab()
        {
            var newPlayerBankAccountButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: openAddBankAccountForm')]"));
            newPlayerBankAccountButton.Click();
            return new PlayerBankAccountForm(_driver);
        }

        public bool IsEditButtonPresent()
        {
            return IsElementPresent(EditButton);
        }
    }

    public class SendNewPlayerPasswordDialog : BackendPageBase
    {
        public SendNewPlayerPasswordDialog(IWebDriver driver) : base(driver) { }

        public string ConfirmationMessage
        {
            get
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
                var confirmationMsg =
                    _driver.FindElementWait(By.XPath("//div[text()='New password has been successfully sent']"));
                wait.Until(x => confirmationMsg.Displayed);
                return confirmationMsg.Text;
            }
        }

        public void SpecifyNewPassword(string newPassword)
        {
            var unselectGenerateNewPasswordCheckbox =
                _driver.FindLastElementWait(By.XPath("//input[contains(@data-bind, 'checked: generateNewPassword')]"));
            unselectGenerateNewPasswordCheckbox.Click();
            var newPasswordField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: newPassword')]"));
            newPasswordField.SendKeys(newPassword);
        }

        public void Send()
        {
            var sendButton = _driver.FindElementWait(By.XPath("//button[text()='Send']"));
            sendButton.Click();
        }
    }

    public class AccountInformation
    {
        public string VIPLevel;
        public string PaymentLevel;
    }
}