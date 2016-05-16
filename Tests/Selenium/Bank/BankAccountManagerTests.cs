using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BankAccountManagerTests : SeleniumBaseForAdminWebsite
    {
        private BankAccountManagerPage _bankAccountManagerPage;
        
        [Test]
        public void Can_edit_bank_account()
        {
            var bankAccountId = TestDataGenerator.GetRandomString(5);
            var bankAccountName = "Bank Account" + TestDataGenerator.GetRandomString(5);
            var bankAccountNumber = TestDataGenerator.GetRandomBankAccountNumber(12);
            const string branchProvince = "branch-province";

            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _bankAccountManagerPage = dashboardPage.Menu.ClickBankAccountsItem();
            var updatedData = TestDataGenerator.EditBankAccountData();
            
            // create a bank account
            var newBankAccountForm = _bankAccountManagerPage.OpenNewBankAccountForm();
            var submittedBankAccountForm = newBankAccountForm.SubmitWithLicensee("Flycow", 
                "138", bankAccountId, bankAccountName, bankAccountNumber, "Bank of Canada", branchProvince);

            // edit bank account details
            submittedBankAccountForm.CloseTab("View Bank Account");
            var editForm = _bankAccountManagerPage.OpenEditForm(bankAccountName);
            var submittedForm = editForm.Submit(data:updatedData, currencyValue:"CNY", bank:"HSBC");

            Assert.AreEqual("The bank account has been successfully updated", submittedForm.ConfirmationMessage);
            Assert.AreEqual(updatedData.ID, submittedBankAccountForm.BankAccountIdValue);
        }

   }
}
