using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class ViewAutoVerificationConfigurationForm : BackendPageBase
    {
        internal static readonly By CloseButtonBy = By.XPath("//button[contains(@data-bind,'close')]");
        internal static readonly By SuccessAlertBy = By.XPath("//div[contains(@class,'alert-success')]");
        internal static readonly By LicenseeBy = By.XPath("//div[contains(@data-bind,'form.fields.licensee')]/p");
        internal static readonly By BrandBy = By.XPath("//div[contains(@data-bind,'form.fields.brand')]/p");
        internal static readonly By CurrencyBy = By.XPath("//div[contains(@data-bind,'form.fields.currency')]/p");
        internal static readonly By VipLevelBy = By.XPath("//div[contains(@data-bind,'form.fields.vipLevel')]/p");


        public IWebElement SuccessAlert
        {
            get { return _driver.FindElementWait(SuccessAlertBy); }
        }

        public IWebElement Licensee
        {
            get { return _driver.FindElementWait(LicenseeBy); }
        }

        public IWebElement Brand
        {
            get { return _driver.FindElementWait(BrandBy); }
        }

        public IWebElement Currency
        {
            get { return _driver.FindElementWait(CurrencyBy); }
        }

        public IWebElement VipLevel
        {
            get { return _driver.FindElementWait(VipLevelBy); }
        }

        public AutoVerificationConfigurationPage CloseAutoVerificationConfigurationForm()
        {
            Click(CloseButtonBy);
            var page = new AutoVerificationConfigurationPage(_driver);
            page.Initialize();
            return page;
        }
        
        public ViewAutoVerificationConfigurationForm(IWebDriver driver)
            : base(driver)
        { }
    }
}
