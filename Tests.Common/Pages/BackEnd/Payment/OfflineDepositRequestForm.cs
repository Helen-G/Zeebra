using System.Globalization;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class OfflineDepositRequestForm : BackendPageBase
    {
        public OfflineDepositRequestForm(IWebDriver driver) : base(driver) { }

        public const string FormXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]/div/div[@data-view='player-manager/offline-deposit/request']";

        public SubmittedOfflineDepositRequestForm Submit(decimal amount)
        {
            var bankList = _driver.FindElementWait(By.XPath(FormXPath + "//*[contains(@id, 'deposit-request-bank')]"));
            NotificationMethod.Click();
            Amount.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            _driver.ScrollPage(0, 800);
            SubmitButton.Click();
            var tab = new SubmittedOfflineDepositRequestForm(_driver);
            tab.Initialize();
            return tab;
        }

        public void TryToSubmit(decimal depositRequestAmount)
        {
            var bankList = _driver.FindElementWait(By.XPath(FormXPath + "//*[contains(@id, 'deposit-request-bank')]"));
            NotificationMethod.Click();
            Amount.SendKeys(depositRequestAmount.ToString(CultureInfo.InvariantCulture));
            _driver.ScrollPage(0, 800);
            SubmitButton.Click();
        }

        public string ValidationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//span[@data-bind='validationMessage: amount']")); }
        }

        public string ErrorMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@data-bind, 'visible: message')]")); }
        }

        public SubmittedOfflineDepositRequestForm SubmitWithBonusCode(string bonusCode, string bonusName, decimal amount)
        {
            //NotificationMethod.Click();
            var amountField = _driver.FindElementWait(By.XPath(FormXPath + "//input[contains(@id, 'deposit-request-amount')]"));
            amountField.SendKeys(amount.ToString(CultureInfo.InvariantCulture));
            var xpath = string.Format("//span[text()='{0}: {1}']", bonusCode, bonusName);
            var bonus = _driver.FindElementWait(By.XPath(xpath));
            bonus.Click();
            _driver.ScrollPage(0, 600);
            var submitButton =
                _driver.FindElementWait(By.XPath(FormXPath + "//button[text()= 'Submit']"));
            submitButton.Click();
            var tab = new SubmittedOfflineDepositRequestForm(_driver);
            return tab;
        }

        public bool BonusIsQualified(string bonusCode, string bonusName)
        {
            var xpath = string.Format("{0}//span[text()='{1}: {2}']", FormXPath, bonusCode, bonusName);
            return _driver.FindElements(By.XPath(xpath)).Any();
        }

        public string Title
        {
            get
            {
                return _driver.FindElementWait(By.XPath("//div[contains(@class, 'nav-tabs-documents')]//ul[contains(@class, 'nav-tabs')]/li[contains(@class, 'active')]/a/span")).Text;
            }
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = FormXPath + "//input[@name='optionsRadios' and @value='SMS']")]
        public IWebElement NotificationMethod { get; set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//*[contains(@id, 'deposit-request-amount')]")]
        public IWebElement Amount { get; private set; }

        [FindsBy(How = How.XPath, Using = FormXPath + "//button[text()= 'Submit']")]
        public IWebElement SubmitButton { get; set; }
#pragma warning restore 649
        
    }
}