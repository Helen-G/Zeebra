using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditPaymentLevelForm : BackendPageBase
    {
        public EditPaymentLevelForm(IWebDriver driver) : base(driver){}

        public SubmittedPaymentLevelForm Submit(string paymentLevelName)
        {
            _paymentLevelNameField.Clear();
            _paymentLevelNameField.SendKeys(paymentLevelName);
            _driver.ScrollPage(0, 700);
            _saveButton.Click();
            var submittedForm = new SubmittedPaymentLevelForm(_driver);
            submittedForm.Initialize();
            return submittedForm;
        }

        
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@data-bind, 'value: fields.name')]")]
        private IWebElement _paymentLevelNameField;

        [FindsBy(How = How.XPath, Using = "//div[@data-view='payments/level-manager/edit']//button[text()='Save']")]
        private IWebElement _saveButton;
#pragma warning restore 649
    }
}