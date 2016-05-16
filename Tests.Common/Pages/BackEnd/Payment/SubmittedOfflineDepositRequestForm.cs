using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedOfflineDepositRequestForm : BackendPageBase
    {
        public SubmittedOfflineDepositRequestForm(IWebDriver driver) : base(driver) {}

        public string Username 
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Username']/following-sibling::div/p")); }
        }

        public string Amount
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Amount']/following-sibling::div/p")); }
        }

        public string Bank
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//label[text()='Bank']/following-sibling::div/p"));
            }
        }

        public string ReferenceCode
        {
            get
            {
                return _driver.FindElementValue(By.XPath(OfflineDepositRequestForm.FormXPath + "//p[contains(@data-bind, 'text: transactionNumber')]"));
            }
        }

        public string NotificationMethod
        {
            get
            {
                return _driver.FindElementValue(By.XPath(OfflineDepositRequestForm.FormXPath + "//p[contains(@data-bind, 'text: notificationMethod')]"));
            }
        }

        public string BankAccountID
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//label[text()='Bank Account ID']/following-sibling::div/p"));
            }
        }

        public string BankAccountName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//label[text()='Bank Account Name']/following-sibling::div/p"));
            }
        }

        public string Confirmation
        {
            get
            {
                return
                    _driver.FindElementValue(By.XPath("//div[@data-view='player-manager/offline-deposit/request']/div"));
            }
        }
    }
}