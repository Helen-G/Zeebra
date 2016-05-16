using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BrandManagerPage : BackendPageBase
    {
        public BrandManagerPage(IWebDriver driver) : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "brandname-search", "brandname-search-button");
            }
        }

        public static By NewButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='New']");
        public static By EditButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='Edit']");
        public static By ActivateButton = By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='Activate']");
        
        public string Title
        {
            get { return _driver.FindElementWait(By.XPath("//h5[text()='Brand Manager']")).Text; } 
        }

        public NewBrandForm OpenNewBrandForm()
        {
            var newBrandButton = _driver.FindElementWait(NewButton);
            newBrandButton.Click();
            var tab = new NewBrandForm(_driver);
            tab.Initialize();
            return tab;
        }

        public EditBrandForm OpenEditBrandForm()
        {
            var editBrandButton = _driver.FindElementWait(By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='Edit']"));
            editBrandButton.Click();
            var tab = new EditBrandForm(_driver);
            tab.Initialize();
            return tab;
        }

        public bool HasNewButton()
        {
            return _driver.FindElementWait(By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='New']")).Displayed;
        }

        public bool HasActiveStatus(string brandName)
        {
            Grid.SelectRecord(brandName);
            return _driver.FindElementWait(By.XPath("//div[@id='brand-grid']//td[@title='Active']")).Displayed;
        }

        public BrandActivateDialog OpenBrandActivateDialog(string brand)
        {
            Grid.SelectRecord(brand);
            var activateButton = _driver.FindElementWait(By.Id("brand-activate-button"));
            activateButton.Click();
            var dialog = new BrandActivateDialog(_driver);
            return dialog;
        }

        public EditBrandForm OpenEditBrandForm(string brand)
        {
            Grid.SelectRecord(brand);
            var editButton = _driver.FindElementWait(EditButton);
            editButton.Click();
            var form = new EditBrandForm(_driver);
            return form;
        }

        public ViewBrandForm OpenViewBrandForm(string brand)
        {
            Grid.SelectRecord(brand);
            var viewButton = _driver.FindElementWait(By.XPath("//div[@data-view='brand/brand-manager/list']//span[text()='View']"));
            viewButton.Click();
            var page = new ViewBrandForm(_driver);
            return page;
        }
    }
}