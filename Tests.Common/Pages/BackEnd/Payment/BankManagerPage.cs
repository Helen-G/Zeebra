using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BankManagerPage : BackendPageBase
    {
        public BankManagerPage(IWebDriver driver) : base(driver) {}

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "banks-name-search", "banks-search-button");
            }
        }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//h5[text()='Banks']")); }
        }

        public NewBankForm OpenNewBankForm()
        {
            var newBankButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='payments/banks/list']//span[text()='New']"));
            newBankButton.Click();
            var tab = new NewBankForm(_driver);
            return tab;
        }

        public EditBankForm OpenEditForm(string bankName)
        {
            Grid.SelectRecord(bankName);
            var editButton = _driver.FindElementWait(By.XPath("//div[@id='banks-home']//button[contains(@data-bind, 'click: openEditTab')]"));
            editButton.Click();
            var editForm = new EditBankForm(_driver);
            return editForm;
        }
    }
}