using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class TransferSettingsPage : BackendPageBase
    {
        public TransferSettingsPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "transfer-settings-name-search", "transfer-settings-search-button");
            }
        }

        public NewTransferSettingsForm OpenNewTransferSettingsForm()
        {
            var newButton =
                _driver.FindElementWait(
                    By.XPath("//div[@id='transfer-settings-home']//button[contains(@data-bind, 'click: openAddTab')]"));
            newButton.Click();
            var form = new NewTransferSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public TransferSettingsActivateDeactivateDialog Activate(string brand, string currency, string viplevel, string remark)
        {
            FindTransferSettingsRecord(brand, currency, viplevel);

            var activateButton =
                _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'enable: canActivate')]"));
            activateButton.Click();

            var dialog = new TransferSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark("remark");
            dialog.Activate();
            return dialog;
        }

        public TransferSettingsActivateDeactivateDialog Deactivate(string brand, string currency, string viplevel, string remark)
        {
            FindTransferSettingsRecord(brand, currency, viplevel);

            var deactivateButton =
                _driver.FindElementWait(
                    By.XPath("//button[contains(@data-bind, 'enable: canDeactivate')]"));
            deactivateButton.Click();

            var dialog = new TransferSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark("remark");
            dialog.Deactivate();
            return dialog;
        }

        public EditTransferSettingsForm OpenEditTransferSettingsForm(string brand, string currency, string viplevel)
        {
            FindTransferSettingsRecord(brand, currency, viplevel);
            var editButton =
                _driver.FindElementWait(
                    By.XPath(
                        "//div[@id='transfer-settings-home']//button[contains(@data-bind, 'click: openDetails(true)')]"));
            editButton.Click();

            var form = new EditTransferSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public void FindTransferSettingsRecord(string brand, string currency, string viplevel)
        {
            Grid.FilterGrid(brand);
            var recordXPath =
                string.Format(
                    "//table[@id='transfer-settings-list']//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}')]",
                    brand, currency, viplevel);
            var recordInGrid = _driver.FindElementWait(By.XPath(recordXPath));
            recordInGrid.Click();
        }

        public string GetStatus(string brand, string currency, string viplevel)
        {
            FindTransferSettingsRecord(brand, currency, viplevel);
            var status = _driver.FindElementValue(By.XPath("//tr[@aria-selected='true']//td[@aria-describedby='transfer-settings-list_Status']"));
            return status;
        }

    }
}
