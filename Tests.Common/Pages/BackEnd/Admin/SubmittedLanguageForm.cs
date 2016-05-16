using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedLanguageForm : BackendPageBase
    {
        public string ConfirmationMessage { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string NativeName { get; private set; }

        public SubmittedLanguageForm(IWebDriver driver) : base(driver)
        {
            ConfirmationMessage = _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']"));
            Code = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: fields.code')]"));
            Name = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: fields.name')]"));
            NativeName = _driver.FindElementValue(By.XPath("//p[contains(@data-bind, 'text: fields.nativeName')]"));
        }
    }
}