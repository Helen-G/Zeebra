using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("For future review")]
    public class FraudManagerTests : SeleniumBaseForAdminWebsite
    {
        private FraudManagerPage _managerPage;
        private RiskLevelTestingDto _current;

        public override void BeforeAll()
        {
            base.BeforeAll();

            var dashboardPage = this._driver.LoginToAdminWebsiteAsSuperAdmin();
            this._managerPage = dashboardPage.Menu.ClickFraudManager();

            // create
            var dto = new RiskLevelTestingDto("Flycow", "138", "RL_" + TestDataGenerator.GetRandomString(5), TestDataGenerator.GetRandomNumber(100), "Remarks_" + TestDataGenerator.GetRandomString(20));
            var submittedForm = this._managerPage.CreateRiskLevel(dto);
            Assert.AreEqual(SubmittedRiskLevelForm.Created, submittedForm.ConfirmationMessage);
            submittedForm.CloseNewForm();

            this._current = dto;
        }

        [Test]
        public void Can_Create_Edit_RiskLevel()
        {
            string newName = this._current.Name + "_edit";
            string newRemarks = this._current.Remarks + "_edit";

            this._managerPage.SearchByRiskLevelName(this._current.Name);

            var submittedForm = this._managerPage.EditRiskLevel(newName, newRemarks);

            Assert.AreEqual(SubmittedRiskLevelForm.Updated, submittedForm.ConfirmationMessage);
            Assert.AreEqual(newName, submittedForm.Name);
            Assert.AreEqual(newRemarks, submittedForm.Remarks);
        }

        [Test]
        public void Can_Activate_Deactivate_RiskLevel()
        {
            this._managerPage.SearchByRiskLevelName(this._current.Name);

            // activate
            string remarks = this._current.Remarks + "_activated";
            this._managerPage.UpdateRiskLevelStatus(true, remarks);

            // deactivate
            remarks = this._current.Remarks + "_deactivated";
            this._managerPage.SearchByRiskLevelName(this._current.Name);
            this._managerPage.UpdateRiskLevelStatus(false, remarks);



        }
    }
}
