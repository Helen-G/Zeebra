using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class RoleManagerPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'admin/role-manager/list')]";

        public RoleManagerPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get { return new Grid(_driver, GridXPath); }
        }
        
        public NewRoleForm OpenNewRoleForm()
        {
            var newRoleButton = FindActionButton("new", GridXPath);
            newRoleButton.Click();
            var form = new NewRoleForm(_driver);
            return form;
        }

        public EditRoleForm OpenEditRoleForm(string roleName)
        {
            Grid.SelectRecord(roleName);
            var editRoleButton = FindActionButton("edit", GridXPath); 
            editRoleButton.Click();
            var form = new EditRoleForm(_driver);
            form.Initialize();
            return form;
        }


        public ViewRoleForm OpenViewRoleForm(string roleName)
        {
            Grid.SelectRecord(roleName);
            var viewRoleButton = FindActionButton("view", GridXPath);
            viewRoleButton.Click();
            var form = new ViewRoleForm(_driver);
            form.Initialize();
            return form;
        }


    }

    
}