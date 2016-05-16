using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BackendIpRegulationsPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'admin/ip-regulations/admin/list')]";

        public BackendIpRegulationsPage(IWebDriver driver)
            : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, GridXPath);
            }
        }

        public NewBackendIpRegulationForm OpenNewBackendIpRegulationForm()
        {
            var newButton = FindActionButton("new", GridXPath);
            newButton.Click();
            var form = new NewBackendIpRegulationForm(_driver);
            form.Initialize();
            return form;
        }

        public EditBackendIpRegulationForm OpenEditBackendIpRegulationForm(string ipAddress)
        {
            Grid.SelectRecord(ipAddress);
            var editButton = FindActionButton("edit", GridXPath);
            editButton.Click();
            var form = new EditBackendIpRegulationForm(_driver);
            form.Initialize();
            return form;
        }

        public void DeleteIpRegulation(string ipAddress)
        {
            Grid.SelectRecord(ipAddress);
            var deleteButton = FindActionButton("delete", GridXPath);
            deleteButton.Click();
            var deleteConfirm =
                _driver.FindElementWait(By.XPath("//div[contains(@class, 'modal-footer')]//button[text() = 'Yes']"));
            deleteConfirm.Click();

            var deleteClose =
                _driver.FindElementWait(By.XPath("//div[contains(@class, 'modal-footer')]//button[text() = 'Close']"));
            deleteClose.Click();
        }

        public bool IsIpRegulationExists(string ipAddress)
        {
            var editButton = FindActionButton("edit", GridXPath);
            return editButton.Enabled;
        }
    }
}