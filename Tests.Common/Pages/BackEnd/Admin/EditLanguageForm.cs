using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditLanguageForm : BackendPageBase
    {
        public EditLanguageForm(IWebDriver driver) : base(driver) { }

        public SubmittedLanguageForm Submit(string newName, string newNativeName)
        {
            var languageName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-name')]"));
            languageName.SendKeys(newName);
            var languageNativeName = _driver.FindElementWait(By.XPath("//input[contains(@id, 'culture-native-name')]"));
            languageNativeName.SendKeys(newNativeName);
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new SubmittedLanguageForm(_driver);
            return submittedForm;
        }
    }
}