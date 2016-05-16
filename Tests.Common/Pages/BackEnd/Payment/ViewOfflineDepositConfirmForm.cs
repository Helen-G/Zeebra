using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ViewOfflineDepositConfirmForm : BackendPageBase
    {
        public ViewOfflineDepositConfirmForm(IWebDriver driver) : base(driver) { }

        public string Username
        {
            get { return _username.Text; }
        }

        public string ReferenceCode
        {
            get { return _referenceCode.Text; }
        }

        public string Amount
        {
            get { return _amount.Text; }
        }

        public string BankAccountName
        {
            get { return _bankAccountName.Text; }
        }

#pragma warning disable 649

        [FindsBy(How = How.XPath, Using = "//label[text()='Username']/following-sibling::div/p")]
        private IWebElement _username;

        [FindsBy(How = How.XPath, Using = "//label[text()='Reference Code']/following-sibling::div/p")]
        private IWebElement _referenceCode;

        [FindsBy(How = How.XPath, Using = "//label[text()='Reference Amount']/following-sibling::div/p")]
        private IWebElement _amount;

        [FindsBy(How = How.XPath, Using = "//label[text()='Bank Account Name']/following-sibling::div/p")]
        private IWebElement _bankAccountName;
#pragma warning restore 649
    }
}