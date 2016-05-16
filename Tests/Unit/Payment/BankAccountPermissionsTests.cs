using System;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    internal class BankAccountPermissionsTests : PermissionsTestsBase
    {
        private BankAccountCommands _bankAccountCommands;
        private BankAccountQueries _bankAccountQueries;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _bankAccountCommands = Container.Resolve<BankAccountCommands>();
            _bankAccountQueries = Container.Resolve<BankAccountQueries>();
        }

        [Test]
        public void Cannot_execute_BankAccountQueries_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.BankAccounts, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountQueries.GetFilteredBankAccounts(new Guid()));
        }

        [Test]
        public void Cannot_execute_BankAccountCommands_without_permissions()
        {
            // Arrange
            LogWithNewUser(Modules.BankAccounts, Permissions.View);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Add(new AddBankAccountData()));
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Edit(new EditBankAccountData()));
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Activate(new Guid(), "Remark"));
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Deactivate(new Guid(), "Remark"));
        }

        [Test]
        public void Cannot_add_bank_account_with_invalid_brand()
        {
            // Arrange
            var data = CreateValidAddBankAccountData();
            LogWithNewUser(Modules.BankAccounts, Permissions.Add);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Add(data));
        }

        [Test]
        public void Cannot_edit_bank_account_with_invalid_brand()
        {
            // Arrange
            var addBankAccountData = CreateValidAddBankAccountData();
            var bankAccount = _bankAccountCommands.Add(addBankAccountData);

            var editBandAccountData = new EditBankAccountData
            {
                Id = bankAccount.Id,
                Bank = bankAccount.Bank.Id,
                Currency = bankAccount.CurrencyCode,
                AccountId = bankAccount.AccountId,
                AccountName = bankAccount.AccountName,
                AccountNumber = bankAccount.AccountNumber,
                AccountType = bankAccount.AccountType,
                Province = bankAccount.Province,
                Branch = bankAccount.Branch,
                Remarks = bankAccount.Remarks
            };

            LogWithNewUser(Modules.BankAccounts, Permissions.Edit);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Edit(editBandAccountData));
        }

        [Test]
        public void Cannot_activate_bank_account_with_invalid_brand()
        {
            // Arrange
            var addBankAccountData = CreateValidAddBankAccountData();
            var bankAccount = _bankAccountCommands.Add(addBankAccountData);
            LogWithNewUser(Modules.BankAccounts, Permissions.Activate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Activate(bankAccount.Id, "Remark"));
        }

        [Test]
        public void Cannot_deactivate_bank_account_with_invalid_brand()
        {
            // Arrange
            var addBankAccountData = CreateValidAddBankAccountData();
            var bankAccount = _bankAccountCommands.Add(addBankAccountData);
            _bankAccountCommands.Activate(bankAccount.Id, "Remark");
            LogWithNewUser(Modules.BankAccounts, Permissions.Deactivate);

            // Act
            Assert.Throws<InsufficientPermissionsException>(() => _bankAccountCommands.Deactivate(bankAccount.Id, "Remark"));
        }

        private AddBankAccountData CreateValidAddBankAccountData()
        {
            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            var paymentTestHelper = Container.Resolve<PaymentTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var brandBankAccount = paymentTestHelper.CreateBankAccount(brand.Id, brand.DefaultCurrency);

            var data = new AddBankAccountData
            {
                Bank = brandBankAccount.Bank.Id,
                Currency = brandBankAccount.CurrencyCode,
                AccountId = brandBankAccount.AccountId,
                AccountName = brandBankAccount.AccountName,
                AccountNumber = brandBankAccount.AccountNumber,
                AccountType = brandBankAccount.AccountType,
                Province = brandBankAccount.Province,
                Branch = brandBankAccount.Branch,
                Remarks = TestDataGenerator.GetRandomString()
            };

            return data;
        }
    }
}