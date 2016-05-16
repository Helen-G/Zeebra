using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class VerifyOfflineDepositForm : BackendPageBase
    {
        public VerifyOfflineDepositForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath = "//div[@data-view='player-manager/offline-deposit/verify']";

        public string Username
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Username']/following-sibling::div/p")).Text; }
        }

        public string ReferenceCode
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Reference Code']/following-sibling::div/p")).Text; }
        }

        public string Amount
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Amount']/following-sibling::div/p")).Text; }
        }

        public string Status
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Status']/following-sibling::div/p")).Text; }
        }

        public string BankName
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Bank Name']/following-sibling::div/p")).Text; }
        }

        public string Remarks
        {
            get { return _driver.FindElementWait(By.Id("verify-deposit-request-remarks-1")).Text; }
        }
        
        public void EnterRemarks(string remark)
        {
            var remarksField = _driver.FindElementWait(By.Id("verify-deposit-request-remarks-0"));
            _driver.ScrollPage(0, 800);
            remarksField.SendKeys(remark);
        }

        public SubmittedDepositVerifyForm Submit()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//button[text()='Submit']"));
            _driver.ScrollPage(0, 800);
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => submitButton.Enabled);
            submitButton.Click();
            var submittedForm = new SubmittedDepositVerifyForm(_driver);
            return submittedForm;
        }

        
    }
}