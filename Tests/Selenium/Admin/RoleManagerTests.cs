using System.Linq;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class RoleManagerTests : SeleniumBaseForAdminWebsite
    {
        private RoleManagerPage _roleManagerPage;
        private DashboardPage _dashboardPage;
        
        const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _roleManagerPage = _dashboardPage.Menu.ClickRoleManagerMenuItem();
        }

        // Can_create_new_role scenario is tested in Copy_permissions_from_role_option_works() test
        
        [Test]
        public void Cannot_create_a_role_without_required_data()
        {
            var newRoleForm = _roleManagerPage.OpenNewRoleForm();
            newRoleForm.Submit();
            var requiredFieldsValidationMessages = newRoleForm.GetValidationMessages();

            Assert.That(requiredFieldsValidationMessages.All(a => a.Contains("is required")));
        }

        [Test]
        public void Can_edit_role()
        {
            //create a role with permissions to create and view users
            var newRoleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var newRoleForm = _roleManagerPage.OpenNewRoleForm();
            newRoleForm.SelectPermissions(new[]
            {
                NewRoleForm.AdminManagerCreate, NewRoleForm.AdminManagerView
            });
               var submittedRoleForm = newRoleForm.FillInRequiredFieldsAndSubmit(newRoleData);

            //update role details
            var updatedRoleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee, description:"updated");
            submittedRoleForm.CloseTab("View Role");
            var editRoleForm = _roleManagerPage.OpenEditRoleForm(newRoleData.RoleName);
            editRoleForm.EditRequiredFields(updatedRoleData.RoleCode, updatedRoleData.RoleName, updatedRoleData.Licensee, updatedRoleData.Description);
            var permissions = (new[] {NewRoleForm.AdminManagerCreate, NewRoleForm.AdminManagerView, NewRoleForm.BrandManagerView});
            editRoleForm.UpdatePermissions(permissions);
            var viewRoleForm = editRoleForm.Submit();
            
            Assert.AreEqual("Role has been successfully updated", viewRoleForm.ConfirmationMessage);
            
            Assert.AreEqual(updatedRoleData.RoleCode, viewRoleForm.RoleCode);
            Assert.AreEqual(updatedRoleData.RoleName, viewRoleForm.RoleName);
            Assert.AreEqual(updatedRoleData.Licensee, viewRoleForm.Licensee);
            Assert.AreEqual("BrandManager", viewRoleForm.Module);
            Assert.That(viewRoleForm.Permission, Is.StringContaining("View"));
        }

        [Test]
        public void Can_view_role_details()
        {
            //create a role with permissions to create and view users
            var roleData = TestDataGenerator.CreateValidRoleData(code: null, name: null, licensee: DefaultLicensee);
            var newRoleForm = _roleManagerPage.OpenNewRoleForm();
            newRoleForm.SelectPermissions(new[] {NewRoleForm.AdminManagerCreate, NewRoleForm.AdminManagerView});
            var submittedRoleForm = newRoleForm.FillInRequiredFieldsAndSubmit(roleData);
            submittedRoleForm.CloseTab("View Role");

            //check role details    
            var viewRoleForm = _roleManagerPage.OpenViewRoleForm(roleData.RoleName);

            Assert.AreEqual("View Role", viewRoleForm.TabName);
            Assert.AreEqual(roleData.RoleCode, viewRoleForm.RoleCode);
            Assert.AreEqual(roleData.RoleName, viewRoleForm.RoleName);
            Assert.AreEqual(roleData.Licensee, viewRoleForm.Licensee);
            Assert.AreEqual("AdminManager", viewRoleForm.Module);
            Assert.That(viewRoleForm.Permissions, Is.StringContaining("Create"));
            Assert.That(viewRoleForm.Permissions, Is.StringContaining("View"));
         }


    }
}
