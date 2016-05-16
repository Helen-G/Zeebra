using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedPaymentLevelForm : BackendPageBase
    {
        public SubmittedPaymentLevelForm(IWebDriver driver) : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            }
        }

        public string IsDefaultValidationMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//span[@id='is-default-validation-message']"));
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//div[@class='alert alert-danger']"));
            }
        }

        public string PaymentLevelCode
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='visible: submitted, text: fields.code']"));
            }
        }

        public string PaymentLevelName
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//p[@data-bind='text: fields.name, visible: submitted']"));
            }
        }

        public bool IsDefaultPaymentLevel
        {
            get
            {
                return _defaultPaymentLevelCheckbox.Selected;
            }
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@id, 'payment-level-default')]")]
        private IWebElement _defaultPaymentLevelCheckbox;
#pragma warning restore 649
    }
}