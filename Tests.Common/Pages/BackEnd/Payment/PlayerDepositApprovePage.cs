using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PlayerDepositApprovePage : BackendPageBase
    {
        public PlayerDepositApprovePage(IWebDriver driver) : base(driver)
        {
        }

        public static By ApproveButton = By.Id("deposit-approve-button");
        public static By RejectButton = By.Id("deposit-approve-reject-button");
        
        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//h5[text()='Player Deposit Approve']")); }
        }

        public bool HasRecordWithReferenceCode(string referenceCode)
        {
            var xpath = string.Format("//div[@id='deposit-approve-grid']//td[text()='{0}']", referenceCode);
            return _driver.FindElements(By.XPath(xpath)).Count == 1;
        }

        public void SelectVerifiedDeposit(string referenceCode)
        {
            var xpath = string.Format("//div[@id='deposit-approve-grid']//td[text()=\"{0}\"]", referenceCode);
            var record = _driver.FindElementWait(By.XPath(xpath));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => record.Displayed);
            record.Click();
        }

        public string SelectVerifiedDepositRequest(string username)
        {
            var userName = FilterGrid(username);
            var depositToConfirm = _driver.FindElementWait(By.XPath(userName));
            depositToConfirm.Click();
            return depositToConfirm.Text;
        }

        public string FilterGrid(string username)
        {
            var searchBox = _driver.FindElementWait(By.Id("deposit-approve-username-search"));
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => searchBox.Displayed);
            searchBox.SendKeys(username);
            var searchButton = _driver.FindElementWait(By.XPath("//button[text()='Search']"));
            searchButton.Click();
            _driver.WaitForJavaScript();
            return string.Format("//td[text() =\"{0}\"]", username);
        }


        public ApproveOfflineDepositForm OpenApproveForm()
        {
            _approveButton.Click();
            _driver.WaitForJavaScript();
            var tab = new ApproveOfflineDepositForm(_driver);
            tab.Initialize();
            return tab;
        }

        public RejectOfflineDepositForm OpenRejectForm()
        {
            var approveButton = _driver.FindElementWait(By.Id("deposit-approve-reject-button"));
            approveButton.Click();
            var tab = new RejectOfflineDepositForm(_driver);
            return tab;
        }
#pragma warning disable 649
        [FindsBy(How = How.Id, Using = "deposit-approve-button")]
        private IWebElement _approveButton;
#pragma warning restore 649
    }
}