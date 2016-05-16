using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AFT.RegoV2.Tests.Common.Extensions;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class EditAutoVerificationConfigurationForm : BackendPageBase
    {
        internal static readonly By SaveButtonBy =
    By.XPath("//button[contains(@class,'btn') and contains(@data-i18n,'save')]");

        internal static readonly By LicenseeBy = By.Id("verification-licensee");
        internal static readonly By BrandBy = By.Id("verification-brand");
        internal static readonly By VipLevelBy = By.Id("verification-vip-level");
        internal static readonly By CurrencyContainerBy = By.Id("verification-currency");

        public ViewAutoVerificationConfigurationForm Submit(AutoVerificationConfigurationData data)
        {         
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


       public EditAutoVerificationConfigurationForm(IWebDriver driver)
            : base(driver)
        {
        }
    }
}