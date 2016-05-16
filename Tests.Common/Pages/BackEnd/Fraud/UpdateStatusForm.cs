using OpenQA.Selenium;
using AFT.RegoV2.Tests.Common.Extensions;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class UpdateStatusForm : BackendPageBase
    {
        private const string XPathModalPrefix = "//div[@class='modal-content']";

        private static readonly By ActivateButton = By.XPath(XPathModalPrefix + "//button[@data-i18n='app:common.activate']");
        private static readonly By DeactivateButton = By.XPath(XPathModalPrefix + "//button[@data-i18n='app:common.deactivate']");
        private static readonly By RemarksCtrl = By.XPath(XPathModalPrefix + "//textarea[@data-bind]");

        public UpdateStatusForm(IWebDriver driver)
            : base(driver)
        { }

        public void SaveSubmit(bool newStatus, string remarks)
        {
            var remarksElement = this._driver.FindLastElementWait(RemarksCtrl);
            remarksElement.Clear();
            remarksElement.SendKeys(remarks);

            this.Click(newStatus ? ActivateButton : DeactivateButton);
        }
    }
}
