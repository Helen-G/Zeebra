using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class BankManagerTests : SeleniumBaseForAdminWebsite
    {
        private BankManagerPage _banksManagerPage;
       
        [Test]
        public void Can_edit_bank()
        {
            var bankName = "Bank" + TestDataGenerator.GetRandomString(3);
            var bankId = TestDataGenerator.GetRandomString(5);
           
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _banksManagerPage = dashboardPage.Menu.ClickBanksItem();
            
            // create a bank
            var newBankForm = _banksManagerPage.OpenNewBankForm();
            var submittedBankForm = newBankForm.SubmitWithLicensee("Flycow", "138", bankId, bankName, "China",
                "new bank");
            submittedBankForm.CloseTab("View Bank");
            
            // edit bank details
            var editForm = _banksManagerPage.OpenEditForm(bankName);
            var newBankName = bankName + "edited";
            var submittedEditForm = editForm.Submit("Flycow", "138", newBankName);

            Assert.AreEqual("The bank has been successfully updated", submittedEditForm.ConfirmationMessage);
            Assert.AreEqual(newBankName, submittedEditForm.BankNameValue);
        }
    }
}
