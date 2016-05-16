using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class FraudManagerPage : BackendPageBase
    {
        private static string XPathPagePrefix = "//div[@data-view='fraud/manager/list']";
        private static readonly By NewButton = By.XPath(XPathPagePrefix + "//span[@data-i18n='app:common.new']");
        private static readonly By EditButton = By.XPath(XPathPagePrefix + "//span[@data-i18n='app:common.edit']");
        private static readonly By ActivateButton = By.XPath(XPathPagePrefix + "//span[@data-i18n='app:common.activate']");
        private static readonly By DeactivateButton = By.XPath(XPathPagePrefix + "//span[@data-i18n='app:common.deactivate']");

        public FraudManagerPage(IWebDriver driver)
            : base(driver)
        { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "fraud-name-search", "fraud-search-button");
            }
        }

        public NewEditRiskLevelForm OpenNewRiskLevelForm()
        {
            this.Click(NewButton);
            var tabPage = new NewEditRiskLevelForm(this._driver);
            tabPage.Initialize();
            return tabPage;
        }

        public NewEditRiskLevelForm OpenEditRiskLevelForm()
        {
            this.Click(EditButton);
            var tabPage = new NewEditRiskLevelForm(this._driver);
            tabPage.Initialize();
            return tabPage;
        }

        public UpdateStatusForm OpenUpdateStatusForm(bool newStatus)
        {
            this.Click(newStatus ? ActivateButton : DeactivateButton);

            var tabPage = new UpdateStatusForm(this._driver);
            tabPage.Initialize();
            return tabPage;
        }

        /// <summary>
        /// search by name and select row
        /// </summary>
        /// <param name="name"></param>
        public void SearchByRiskLevelName(string name)
        {
            Grid.SelectRecord(name);
        }

        public SubmittedRiskLevelForm CreateRiskLevel(RiskLevelTestingDto dto)
        {
            var newForm = this.OpenNewRiskLevelForm();
            return newForm.NewSubmit(dto);
        }

        public SubmittedRiskLevelForm EditRiskLevel(string newName, string newRemarks)
        {
            var editForm = this.OpenEditRiskLevelForm();
            return editForm.EditSubmit(newName, newRemarks);
        }

        public void UpdateRiskLevelStatus(bool newStatus, string remarks)
        {
            var statusForm = this.OpenUpdateStatusForm(newStatus);

            statusForm.SaveSubmit(newStatus, remarks);
        }
    }
}
