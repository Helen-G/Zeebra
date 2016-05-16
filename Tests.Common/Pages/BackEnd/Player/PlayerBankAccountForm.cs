using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerBankAccountForm : BackendPageBase
    {
        public PlayerBankAccountForm(IWebDriver driver)
            : base(driver)
        {
        }

        public SubmittedPlayerBankAccountForm Submit(PlayerBankAccountData data)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(45));
            var provinceField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-province')]"));
            wait.Until(d => provinceField.Displayed && provinceField.Enabled);

            provinceField.SendKeys(data.Province);

            var cityField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-city')]"));
            cityField.SendKeys(data.City);

            var branchField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-branch')]"));
            branchField.SendKeys(data.Branch);

            var swiftField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-swift-code')]"));
            swiftField.SendKeys(data.SwiftCode);

            var addressField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-address')]"));
            addressField.SendKeys(data.Address);

            var bankAccountNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-account-name')]"));
            bankAccountNameField.SendKeys(data.BankAccountName);

            var bankAccountNumberField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'player-bank-account-account-number')]"));
            bankAccountNumberField.SendKeys(data.BankAccountNumber);

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();

            var form = new SubmittedPlayerBankAccountForm(_driver);
            form.Initialize();

            return form;
        }
    }
}