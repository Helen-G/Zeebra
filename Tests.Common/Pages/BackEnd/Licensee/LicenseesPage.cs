using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class LicenseeManagerPage : BackendPageBase
    {
        public LicenseeManagerPage(IWebDriver driver) : base(driver){}

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "licensee-name-search", "licensee-name-search-button");
            }
        }

        public NewLicenseeForm OpenNewLicenseeForm()
        {
            var newButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='licensee-manager/list']//span[text()='New']"));
            newButton.Click();
            var form = new NewLicenseeForm(_driver);
            return form;
        }

        public EditLicenseeForm OpenEditLicenseeForm(string licensee)
        {
            Grid.SelectRecord(licensee);
            var editButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='licensee-manager/list']//span[text()='Edit']"));
            editButton.Click();
            var form = new EditLicenseeForm(_driver);
            form.Initialize();
            return form;
        }

        public ViewLicenseeForm OpenViewLicenseeForm(string licensee)
        {
            Grid.SelectRecord(licensee);
            var editButton =
                _driver.FindElementWait(By.XPath("//div[@data-view='licensee-manager/list']//span[text()='View']"));
            editButton.Click();
            var form = new ViewLicenseeForm(_driver);
            return form;
        }

        public LicenseeActivateDialog OpenActivateLicenseeDialog(string licensee)
        {
            Grid.SelectRecord(licensee);
            var activateButton = _driver.FindElementWait(By.Id("licensee-activate-button"));
            activateButton.Click();
            var dialog = new LicenseeActivateDialog(_driver);
            return dialog;
        }

        public RenewContractForm OpenRenewContractForm(string licensee)
        {
            Grid.SelectRecord(licensee);
            var editButton =
                _driver.FindElementWait(By.Id("licensee-renew-contract-button"));
            editButton.Click();
            var form = new RenewContractForm(_driver);
            form.Initialize();
            return form;
        }
    }

    public class RenewContractForm : BackendPageBase
    {
        public RenewContractForm(IWebDriver driver) : base(driver) {}

        public ViewLicenseeForm Submit(string newContractDate, string contractEnd = null)
        {
            _newContractStart.Clear();
            _newContractStart.SendKeys(newContractDate);
            if (!string.IsNullOrEmpty(contractEnd))
            {
                _newContractEnd.Clear();
                _newContractEnd.SendKeys(contractEnd);
            }
            _saveButton.Click();
            var form = new ViewLicenseeForm(_driver);
            form.Initialize();
            return form;
        }

#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[contains(@id, 'licensee-contract-contract-start')]")]
        private IWebElement _newContractStart;

        [FindsBy(How = How.XPath, Using = "//input[contains(@id, 'licensee-contract-contract-end')]")]
        private IWebElement _newContractEnd;

        [FindsBy(How = How.XPath, Using = "//button[text()='Save']")]
        private IWebElement _saveButton;
#pragma warning restore 649

    }
}
