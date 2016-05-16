using System;
using System.Globalization;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Actions = OpenQA.Selenium.Interactions.Actions;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ApproveOfflineDepositForm : BackendPageBase
    {
        public ApproveOfflineDepositForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath = "//div[@data-view='player-manager/offline-deposit/approve']";
        
        public string Username
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Username']/following-sibling::div/p")); }
        }

        public string ReferenceCode
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Reference Code']/following-sibling::div/p")); }
        }

        public string Status
        {
            get { return _driver.FindElementWait(By.XPath("//label[text()='Status']/following-sibling::div/p")).Text; }
        }

        public string BankName
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Bank Name']/following-sibling::div/p")); }
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath(FormXPath + "/div")); }
            
        }

        public void Approve(string amount, string fee)
        {
            _driver.ScrollPage(0, 1200); // if the element is on the bottom.
            var actualAmountField = _driver.FindElementWait(By.Id("deposit-approval-amount-0"));
            actualAmountField.SendKeys(amount); 
            var feeField = _driver.FindElementWait(By.Id("deposit-approval-fee-0"));
            feeField.SendKeys(fee);
            var submitButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Approve']"));
            new Actions(_driver).MoveToElement(submitButton);
            submitButton.Click();
            _driver.ScrollPage(1200, 0);
        }

        public void Approve(decimal amount, string fee)
        {
            _driver.ScrollPage(0, 1200); // if the element is on the bottom.
            var actualAmountField = _driver.FindElementWait(By.Id("deposit-approval-amount-0"));
            var amountValue = amount.ToString();
            actualAmountField.SendKeys(amountValue);
            var feeField = _driver.FindElementWait(By.Id("deposit-approval-fee-0"));
            feeField.SendKeys(fee);
            var submitButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Approve']"));
            submitButton.Click();
            _driver.ScrollPage(1200, 0);
        }

        public SubmittedDepositApproveForm Submit(decimal amount, string fee)
        {
            _driver.ScrollPage(0, 1200); // if the element is on the bottom.
            var amountLabel = _driver.FindElementWait(By.XPath("//label[contains(@for, 'deposit-approval-amount')]"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(x => amountLabel.Displayed);
            Amount.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            Fee.SendKeys(fee);
            _driver.ScrollPage(0, 800);
            SubmitButton.Click();
            var tab = new SubmittedDepositApproveForm(_driver);
            tab.Initialize();
            return tab;
        }

        [FindsBy(How = How.XPath, Using = FormXPath + "//*[contains(@id, 'deposit-approval-amount-0')]")]
        public IWebElement Amount { get; private set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//*[contains(@id, 'deposit-approval-fee-0')]")]
        public IWebElement Fee { get; private set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[text()= 'Approve']")]
        public IWebElement SubmitButton { get; set; }

    }
}