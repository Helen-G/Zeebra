using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BankAccountManagerPage : BackendPageBase
    {
        public BankAccountManagerPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "bank-accounts-name-search", "bank-accounts-search-button");
            }
        }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//h5[text()='Bank Accounts']")); }
            
        }

        public NewBankAccountForm OpenNewBankAccountForm()
        {
            var newButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/list']//span[text()='New']"));
            newButton.Click();
            var form = new NewBankAccountForm(_driver);
            return form;
        }

        public ActivateBankAccountDialog OpenActivateBankAccountDialog(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var activateButton = _driver.FindElementWait(By.XPath("//div[@data-view='payments/bank-accounts/list']//button[contains(@data-bind, 'enable: canActivate')]"));
            activateButton.Click();
            var dialog = new ActivateBankAccountDialog(_driver);
            return dialog;
        }

        public EditBankAccountForm OpenEditForm(string bankAccountName)
        {
            Grid.SelectRecord(bankAccountName);
            var editButton = _driver.FindElementWait(By.Id("bank-accounts-edit-btn"));
            editButton.Click();
            var form = new EditBankAccountForm(_driver);
            form.Initialize();
            return form;
        }
    }
}