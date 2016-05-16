using System.Collections.Generic;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewRoleForm : BackendPageBase
    {
        public NewRoleForm(IWebDriver driver) : base(driver) {}

        // permission checkboxes on permissions tree
        public const string AdminManagerCreate = "root-adminmanager-create";
        public const string AdminManagerView = "root-adminmanager-view";

        public const string RoleManagerView = "root-rolemanager-view";
        public const string RoleManagerCreate = "root-rolemanager-create";
        
        public const string OfflineDepositRequestCreate = "root-offlinedepositrequests-create";
        public const string OfflineDepositRequestView = "root-offlinedepositrequests-view";

        public const string PlayerManagerView = "root-playermanager-view";
        public const string PlayerManagerCreate = "root-playermanager-create";
        public static string PlayerManagerEdit = "root-playermanager-update";
        public static string AssignVipLevelToPlayer = "root-playermanager-assignviplevel";
        
        public const string OfflineDepositConfirmationConfirm = "root-offlinedepositconfirmation-confirm";
        public const string OfflineDepositConfirmationView = "root-offlinedepositconfirmation-view";
        public const string OfflineDepositVerificationView = "root-offlinedepositverification-view";
        public const string OfflineDepositVerificationActivate = "root-offlinedepositverification-activate";
        public const string OfflineDepositVerificationDeactivate = "root-offlinedepositverification-activate";

        public const string LanguageManagerView = "root-languagemanager-view";
        public const string LanguageManagerActivate = "root-languagemanager-activate";

        public const string OfflineWithdrawCreate = "root-offlinewithdrawalrequest-create";
        public const string OfflineWithdrawVerify = "root-offlinewithdrawalverification-verify";
        public const string OfflineWithdrawUnverify = "root-offlinewithdrawalverification-unverify";
        public const string OfflineWithdrawView = "root-offlinewithdrawalrequest-view";
        
        public const string BonusTemplateManagerCreate = "root-bonustemplatemanager-create";
        public const string BonusTemplateManagerView = "root-bonustemplatemanager-view";

        public const string BrandManagerView = "root-brandmanager-view";
        public const string BrandManagerCreate = "root-brandmanager-create";
        public const string BrandManagerUpdate = "root-brandmanager-update";
        

        public SubmittedRoleForm Submit(string licensee, string brand, string code, string name, string permission)
        {
            var roleCode = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.code')]"));
            roleCode.SendKeys(code);
            var roleName =
                _driver.FindElementWait(By.XPath("//div[@id='add-role-home']//input[contains(@data-bind, 'value: Model.name')]"));
            roleName.SendKeys(name);

            var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
            licenseesWidget.SelectFromMultiSelect(licensee);

            _driver.ScrollPage(0, 900);
            var rolePermission =
                _driver.FindElementWait(By.XPath(permission)); 
            rolePermission.Click();
            _driver.ScrollPage(0, 1600);
            var submitButton = _driver.FindElementWait(By.XPath("//div[@id='add-role-home']//button[text()='Save']"));
            submitButton.Click();
            var submittedForm = new SubmittedRoleForm(_driver);
            return submittedForm;
        }

        public void CopyPermissionsFromAnotherRole(string role)
        {
            var rolesListField = _driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'options: Model.roles')]"));
            var rolesdropDown = new SelectElement(rolesListField);
            rolesdropDown.SelectByText(role);
        }

        public void SelectPermissions(string[] permissions)
        {
           _driver.ScrollPage(0, 900);

            ExpandPermissions();

           foreach (var permission in permissions)
           {
               var elementXPath = string.Format("//tr[td/@title='{0}']//input", permission);
               _driver.FindElementClick(elementXPath);
           }
        }

        public void ExpandPermissions()
        {
            const string expandButtonXPath = "//button[contains(@data-bind, 'click: expandGrid')]";
            _driver.FindElementClick(expandButtonXPath);
        }

        public SubmittedRoleForm FillInRequiredFields(RoleData data)
        {
            var roleCode = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'value: Model.code')]"));
            roleCode.SendKeys(data.RoleCode);
            var roleName =
                _driver.FindElementWait(By.XPath("//div[@id='add-role-home']//input[contains(@data-bind, 'value: Model.name')]"));
            roleName.SendKeys(data.RoleName);
            var licenseesWidget = new MultiSelectWidget(_driver, By.XPath("//div[contains(@data-bind, 'items: Model.assignedLicensees')]"));
            licenseesWidget.SelectFromMultiSelect(data.Licensee);

            
            var submittedForm = new SubmittedRoleForm(_driver);
            return submittedForm;
        }

        public SubmittedRoleForm FillInRequiredFieldsAndSubmit(RoleData data)
        {
            var submittedForm = FillInRequiredFields(data);
            Submit();

            return submittedForm;
        }

        public void Submit()
        {
            var submitButton = _driver.FindElementWait(By.XPath("//div[@id='add-role-home']//button[text()='Save']"));
            submitButton.Click();
        }

        public IEnumerable<string> GetValidationMessages()
        {
            var roleCode = _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'validationMessage: Model.code')]"));
            yield return roleCode;
            var roleName = _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'validationMessage: Model.name')]"));
            yield return roleName;
            var licensee = _driver.FindElementValue(By.XPath("//span[contains(@data-bind, 'validationMessage: Model.assignedLicensees')]"));
            yield return licensee;
        }
    }
}