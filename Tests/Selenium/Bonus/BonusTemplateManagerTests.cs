using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BonusTemplateManagerTests : SeleniumBaseForAdminWebsite
    {
        private BonusTemplateManagerPage _bonusTemplateManagerPage;

        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bonusTemplateManagerPage = dashboardPage.Menu.ClickBonusTemplateMenuItem();
        }

        [Test]
        public void Can_delete_bonus_template()
        {
            // create a bonus template
            var bonusTemplateName = "Deposit-Bonus-Template" + TestDataGenerator.GetRandomString(3);
            CreateBonusTemplate(bonusTemplateName);

            // delete the bonus template
            var deleteDialog = _bonusTemplateManagerPage.OpenDeleteBonusTemplateDialog(bonusTemplateName);
            _bonusTemplateManagerPage = deleteDialog.Confirm();
            var deletedBonusTemplate = _bonusTemplateManagerPage.SearchForDeletedRecord(bonusTemplateName);

            Assert.False(deletedBonusTemplate);
        }

        [Test]
        public void Can_edit_bonus_template()
        {
            // create a bonus template
            var bonusTemplateName = "Bonus-Template" + TestDataGenerator.GetRandomString(3);
            CreateBonusTemplate(bonusTemplateName);

            // edit the bonus template
            var newTemplateName = bonusTemplateName + "edited";
            var summaryPage = _bonusTemplateManagerPage.OpenEditForm(bonusTemplateName)
                .SetTemplateName(newTemplateName)
                .NavigateToRules()
                .NavigateToSummary();

            Assert.AreEqual(newTemplateName, summaryPage.Name);
        }

        [Test]
        public void Can_create_and_view_bonus_template()
        {
            // create a bonus template
            var bonusTemplateName = "Bonus-Template" + TestDataGenerator.GetRandomString(3);
            CreateBonusTemplate(bonusTemplateName);

            // view bonus template details
            var viewForm = _bonusTemplateManagerPage.OpenViewForm(bonusTemplateName);

            Assert.AreEqual(bonusTemplateName, viewForm.Name);
            Assert.AreEqual("138", viewForm.Brand);
        }

        private void CreateBonusTemplate(string bonusTemplateName)
        {
            _bonusTemplateManagerPage.OpenNewBonusTemplateForm()
                .SelectLicenseeAndBrand(DefaultLicensee, DefaultBrand)
                .SetTemplateName(bonusTemplateName)
                .NavigateToRules()
                .SelectCurrency("CNY")
                .EnterBonusTier(100)
                .NavigateToSummary()
                .CloseTab();
        }
    }
}
