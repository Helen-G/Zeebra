using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class PaymentSettingsPage : BackendPageBase
    {
        public PaymentSettingsPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "payment-settings-name-search", "payment-settings-search-button");
            }
        }

        public string GetStatus(string brand, string currency, string viplevel)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);
            var status = _driver.FindElementValue(By.XPath("//tr[@aria-selected='true']//td[@aria-describedby='payment-settings-list_Status']"));
            return status;
        }

        public NewPaymentSettingsForm OpenNewPaymentSettingsForm()
        {
            var newButton =
                _driver.FindElementWait(
                    By.XPath("//div[@id='payment-settings-home']//button[contains(@data-bind, 'click: openAddTab')]"));
            newButton.Click();
            var form = new NewPaymentSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public PaymentSettingsActivateDeactivateDialog Activate(string brand, string currency, string viplevel, string remark)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);

            var activateButton =
                _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'enable: canActivate')]"));
            activateButton.Click();
            var dialog = new PaymentSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark("remark");
            dialog.Activate();
            return dialog;
        }

        public PaymentSettingsActivateDeactivateDialog Deactivate(string brand, string currency, string viplevel, string remark)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);

            var deactivateButton =
                _driver.FindElementWait(
                    By.XPath("//button[contains(@data-bind, 'enable: canDeactivate')]"));
            deactivateButton.Click();

            var dialog = new PaymentSettingsActivateDeactivateDialog(_driver);
            dialog.EnterRemark("remark");
            dialog.Deactivate();
            return dialog;
        }


        public EditPaymentSettingsForm OpenEditPaymentSettingsForm(string brand, string currency, string viplevel)
        {
            FindPaymentSettingsRecord(brand, currency, viplevel);
            var editButton =
                _driver.FindElementWait(
                    By.XPath(
                        "//div[@id='payment-settings-home']//button[contains(@data-bind, 'click: openDetails(true)')]"));
            editButton.Click();

            var form = new EditPaymentSettingsForm(_driver);
            form.Initialize();
            return form;
        }

        public void FindPaymentSettingsRecord(string brand, string currency, string viplevel)
        {
            Grid.FilterGrid(brand);
            var recordXPath =
                string.Format(
                    "//table[@id='payment-settings-list']//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}')]",
                    brand, currency, viplevel);
            var recordInGrid = _driver.FindElementWait(By.XPath(recordXPath));
            recordInGrid.Click();
        }
    }
}