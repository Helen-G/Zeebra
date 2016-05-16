using System.Web.Configuration;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class AccountActivationPage : FrontendPageBase
    {
        private readonly string _verificationToken;

        public AccountActivationPage(IWebDriver driver, string verificationToken) : base(driver)
        {
            _verificationToken = verificationToken;
        }

        protected override string GetPageUrl()
        {
            return "en-US/Home/Activate?token=" + _verificationToken;
        }

        public void NavigateTo()
        {
            var url = WebConfigurationManager.AppSettings["MemberWebsiteUrl"] + GetPageUrl();
            _driver.Navigate().GoToUrl(url);
        }
    }
}