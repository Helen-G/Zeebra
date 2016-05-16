using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ProductManagerPage : BackendPageBase
    {
        public ProductManagerPage(IWebDriver driver) : base(driver) {}

        public NewProductForm OpenNewProductForm()
        {
            var newButton = _driver.FindElementWait(By.XPath(" //div[@id='game-providers-grid']//button[contains(@data-bind, 'click: openAddGameProvider')]"));
            newButton.Click();
            var form = new NewProductForm(_driver);
            return form;
        }
    }
}