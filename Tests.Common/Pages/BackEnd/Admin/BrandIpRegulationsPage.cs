using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BrandIpRegulationsPage : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'admin/ip-regulations/brand/list')]";

        public BrandIpRegulationsPage(IWebDriver driver) : base(driver) { }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, GridXPath);
            }
        }

        public NewBrandIpRegulationForm OpenNewBrandIpRegulationForm()
        {
            var newButton = FindActionButton("new", GridXPath);
            newButton.Click();
            var form = new NewBrandIpRegulationForm(_driver);
            form.Initialize();
            return form;
        }

        public EditBrandIpRegulationForm OpenEditBrandIpRegulationForm(string ipAddress)
        {
            Grid.SelectRecord(ipAddress);
            var editButton = FindActionButton("edit", GridXPath);
            editButton.Click();
            var form = new EditBrandIpRegulationForm(_driver);
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

public class BrandIpRegulationData
{
    public string Licensee { get; set; }
    public string Brand { get; set; }
    public string IpAddress { get; set; }
    public bool AdvancedSettings { get; set; }
    public string MultipleIpAddress { get; set; }
    public string Description { get; set; }
    public string Restriction { get; set; }
    public string BlockingType { get; set; }
    public string RedirectUrl { get; set; }
}