using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class UnverifyOfflineDepositForm : BackendPageBase
    {
        public UnverifyOfflineDepositForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath = "//div[@data-view='player-manager/offline-deposit/verify']";

        public string Username
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Username']/following-sibling::div/p")); }
        }

        public string ReferenceCode
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Reference Code']/following-sibling::div/p")); }
        }

        public string Amount
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Amount']/following-sibling::div/p")); }
        }

        public string Status
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Status']/following-sibling::div/p")); }
        }

        public string BankName
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Bank Name']/following-sibling::div/p")); }
        }

        public string Remarks
        {
            get { return _driver.FindElementValue(By.Id("verify-deposit-request-remarks-3")); }
        }

        public void EnterRemarks(string remark)
        {
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'verify-deposit-request-Remark')]"));
            _driver.ScrollPage(0, 800);
            remarksField.SendKeys(remark);
        }

        public void Submit()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Unverify']"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => submitButton.Enabled);
            submitButton.Click();
        }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@data-view='player-manager/offline-deposit/verify']/div"));
            }
        }
    }
}