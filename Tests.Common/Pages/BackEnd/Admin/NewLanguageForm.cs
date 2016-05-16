using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewLanguageForm : BackendPageBase
    {
        public NewLanguageForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedLanguageForm Submit(string code, string name, string nativeName)
        {
            var languageCode = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-code')]"));
            languageCode.SendKeys(code);
            var languageName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-name')]"));
            languageName.SendKeys(name);
            var languageNativeName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-native-name')]"));
            languageNativeName.SendKeys(nativeName);
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedLanguageForm(_driver);
            return submittedForm;
        }

        public SubmittedLanguageForm Submit(LanguageData data)
        {
            var languageCode = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-code')]"));
            languageCode.SendKeys(data.LanguageCode);
            var languageName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-name')]"));
            languageName.SendKeys(data.LanguageName);
            var languageNativeName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-native-name')]"));
            languageNativeName.SendKeys(data.NaviteName);
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedLanguageForm(_driver);
            return submittedForm;
        }
    }
}