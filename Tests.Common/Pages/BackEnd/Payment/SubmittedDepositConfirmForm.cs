using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedDepositConfirmForm : BackendPageBase
    {
        public SubmittedDepositConfirmForm(IWebDriver driver) : base(driver)
        {

        }

        public string GetConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]" +
                                                         "/div/div[@data-view='player-manager/offline-deposit/confirm']/div[@class='alert alert-success']"));
            }
        }

        public string GetErrorMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@data-view='player-manager/offline-deposit/confirm']//div[@class='alert alert-danger']")); }
        }

        public string Username
        {
            get { return _driver.FindElementValue(By.XPath(ConfirmOfflineDepositForm.BaseXPath + "//*[contains(@data-bind, 'text: username')]")); }

        }

        public string ReferenceCode
        {
            get { return _driver.FindElementValue(By.XPath(ConfirmOfflineDepositForm.BaseXPath + "//*[contains(@data-bind, 'text: transactionNumber')]")); }
        }

        public string BankAccountID
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Bank Account ID']/following-sibling::div/p")); }
        }

        public string Amount
        {
            get { return _driver.FindElementValue(By.XPath(ConfirmOfflineDepositForm.BaseXPath + "//*[contains(@data-bind, 'text: amount')]")); }
        }

        public string Remark
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//label[text()='Remark']/following-sibling::div/p"));
            }
        }

       
    }
}