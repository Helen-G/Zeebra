using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class LanguageManagerTests : SeleniumBaseForAdminWebsite
    {
        private LanguageManagerPage _languages;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _languages = dashboardPage.Menu.ClickLanguageManagerMenuItem();
        }

        [Test]
        public void Can_create_a_language()
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var code = TestDataGenerator.GetRandomAlphabeticString(3);
            var name = "Name" + code;
            var nativeName = "NativeName" + code;
            var submittedLanguageForm = newLanguageForm.Submit(code, name, nativeName);

            Assert.AreEqual("The language has been successfully created", submittedLanguageForm.ConfirmationMessage);
        }

        [Test]
        public void Can_edit_a_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");
                        
            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var editForm = _languages.OpenEditLanguageForm();
            var newSuffix = TestDataGenerator.GetRandomAlphabeticString(3);
            var newName = "Name" + newSuffix;
            var newNativeName = "NativeName" + newSuffix;
            var submittedEditForm = editForm.Submit(newName, newNativeName);

            Assert.AreEqual("The language has been successfully updated", submittedEditForm.ConfirmationMessage);
        }

        [Test]
        public void Can_view_a_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");
            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var viewPage = _languages.OpenViewLanguagePage();

            Assert.AreEqual("View Language", viewPage.TabName);
        }

        [Test]
        public void Can_deactivate_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var dialog = _languages.ShowDeactivateDialog();
            dialog.Deactivate();

            Assert.AreEqual("The language has been successfully deactivated.", dialog.ResponseMessage);
        }

        [Test]
        public void Can_activate_language()
        {
            var newLanguage = CreateLanguage();
            newLanguage.CloseTab("View Language");

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var deactivateDialog = _languages.ShowDeactivateDialog();
            deactivateDialog.Deactivate();
            deactivateDialog.CloseDialog();

            _driver.SelectRecordInGridExtendFilter("Name", "is", newLanguage.Name);
            var acctivateDialog = _languages.ShowActivateDialog();
            acctivateDialog.Activate();
            acctivateDialog.CloseDialog();

            Assert.AreEqual("The language has been successfully activated.", acctivateDialog.ResponseMessage);
        }

        public SubmittedLanguageForm CreateLanguage(string code = null, string name = null, string nativeName = null)
        {
            var newLanguageForm = _languages.OpenNewLanguageForm();
            var newCode = code ?? TestDataGenerator.GetRandomAlphabeticString(3);
            var newName = name ?? "Name" + newCode;
            var newNativeName = nativeName ?? "NativeName" + newCode;
            return newLanguageForm.Submit(newCode, newName, newNativeName);
        }
    }
}
