using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class LanguageManagerPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'culture-manager/list')]";

        public LanguageManagerPage(IWebDriver driver) : base(driver)
        {
        }

        public NewLanguageForm OpenNewLanguageForm()
        {
            var newButton = FindActionButton("new", GridXPath);
            newButton.Click();
            var form = new NewLanguageForm(_driver);
            return form;
        }

        public EditLanguageForm OpenEditLanguageForm()
        {
            var editButton = FindActionButton("edit", GridXPath);
            editButton.Click();
            var editForm = new EditLanguageForm(_driver);
            return editForm;
        }

        public ViewLanguagePage OpenViewLanguagePage()
        {
            var viewButton = FindActionButton("view", GridXPath);
            viewButton.Click();
            return new ViewLanguagePage(_driver);
        }

        public ActivateDeactivateLanguageDialog ShowDeactivateDialog()
        {
            var deactivateButton = FindActionButton("deactivate", GridXPath);
            deactivateButton.Click();
            return new ActivateDeactivateLanguageDialog(_driver);
        }

        public ActivateDeactivateLanguageDialog ShowActivateDialog()
        {
            var activateButton = FindActionButton("activate", GridXPath);
            activateButton.Click();
            return new ActivateDeactivateLanguageDialog(_driver);
        }


        public IWebElement GetButton(string btnName)
        {
            return FindActionButton(btnName, GridXPath);
        }
    }

    public class ViewLanguagePage : BackendPageBase
    {
        public ViewLanguagePage(IWebDriver driver) : base(driver)
        {
        }
    }
}
