using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class RejectOfflineDepositForm : BackendPageBase
    {
        public RejectOfflineDepositForm(IWebDriver driver) : base(driver)
        {
        }

        public const string FormXPath = "//div[@data-view='player-manager/offline-deposit/approve']";

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

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath(FormXPath + "/div")); }

        }

        public SubmittedRejectOfflineDepositForm Submit(string amount, string fee)
        {
            _driver.ScrollPage(0, 700);
            var actualAmountField = _driver.FindElementWait(By.Id("deposit-approval-amount-0"));
            actualAmountField.SendKeys(amount); 
            var feeField = _driver.FindElementWait(By.Id("deposit-approval-fee-0"));
            feeField.SendKeys(fee);
            var submitButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[text()='Reject']"));
            submitButton.Click();
            var form = new SubmittedRejectOfflineDepositForm(_driver);
            return form;
        }
    }
}