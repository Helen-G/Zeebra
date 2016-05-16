using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AFT.RegoV2.Tests.Common.Extensions;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class NewAutoVerificationConfigurationForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy =
            By.XPath("//button[contains(@class,'btn') and contains(@data-i18n,'save')]");

        internal static readonly By LicenseeBy = By.Id("verification-licensee");
        internal static readonly By BrandBy = By.Id("verification-brand");
        internal static readonly By VipLevelBy = By.Id("verification-vip-level");
        internal static readonly By CurrencyContainerBy = By.Id("verification-currency");

 
        public ViewAutoVerificationConfigurationForm SubmitAutoVerificationConfiguration(
            AutoVerificationConfigurationData data)
        {
            //Set licensee
            IWebElement licenseeDropdown = null;
            try
            {
                var e = _driver.FindElement(LicenseeBy);
                if (e.Displayed)
                    licenseeDropdown = e;
            }
            catch (NoSuchElementException)
            {
            }
            if (licenseeDropdown != null)
                new SelectElement(licenseeDropdown).SelectByText(data.Licensee);

            //Set Brand
            new SelectElement(_driver.FindElementWait(BrandBy)).SelectByText(data.Brand);

            //Set Currency
            new SelectElement(_driver.FindElementWait(CurrencyContainerBy)).SelectByText(data.Currency);

            //Set VIP level
            if (data.VipLevel != null)
            {
                new SelectElement(_driver.FindElementWait(VipLevelBy)).SelectByText(data.VipLevel);
            }

            Click(SaveButtonBy);
            var page = new ViewAutoVerificationConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

        public NewAutoVerificationConfigurationForm(IWebDriver driver)
            : base(driver)
        {
        }

    }

}