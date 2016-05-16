using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class BetLevelsPage : BackendPageBase
    {
        public BetLevelsPage(IWebDriver driver) : base(driver) {}

        public NewBetLevelForm OpenNewBetLevelForm()
        {
            var newButton = _driver.FindElementWait(By.Id("btn-add-product-bet-level"));
            newButton.Click();
            var newForm = new NewBetLevelForm(_driver);
            return newForm;
        }
    }

    public class NewBetLevelForm : BackendPageBase
    {
        public NewBetLevelForm(IWebDriver driver) : base(driver) {}

        public void AddBetLevelDetails(string name, string code)
        {
            const string namePath = "(//input[contains(@id, 'name-input')])[last()]";
            const string codePath = "(//input[contains(@id, 'code-input')])[last()]";

            var betLevels = _driver.FindElements(By.XPath(namePath));
            var originalBetLevelCount = betLevels.Count;

            if (originalBetLevelCount > 1)
            {
                var addBetButton = _driver.FindElementScroll(By.XPath("//button[contains(@data-bind, 'click: addBetLevel')]"));
                addBetButton.Click();

                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
                wait.Until(x => _driver.FindElements(By.XPath(namePath)).Count > originalBetLevelCount);
            }
           
            var betLevelName = _driver.FindElementWait(By.XPath(namePath));
            betLevelName.Clear();
            betLevelName.SendKeys(name);

            var betLevelCode = _driver.FindElementWait(By.XPath(codePath));
            betLevelCode.Clear();
            betLevelCode.SendKeys(code);
        }

        public ViewBetLevelForm Submit()
        {
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedBetLevelForm = new ViewBetLevelForm(_driver);
            return submittedBetLevelForm;
        }


        public void SelectLicensee(string licenseeName)
        {
            var licenseeList = _driver.FindElementWait(By.XPath("//div[@data-view='product/bet-levels/edit']//select[@id='licensee-select']"));
            var licenseeField = new SelectElement(licenseeList);
            licenseeField.SelectByText(licenseeName);
        }

        public void SelectBrand(string brandName)
        {
            var brandList = _driver.FindElementWait(By.XPath("//div[@data-view='product/bet-levels/edit']//select[@id='brand-select']"));
            var brandField = new SelectElement(brandList);
            brandField.SelectByText(brandName);
        }

        public void SelectProduct(string productName)
        {
            var productsList = _driver.FindElementWait(By.XPath("//div[@data-view='product/bet-levels/edit']//select[@id='product-select']"));
            var productField = new SelectElement(productsList);
            productField.SelectByText(productName);
        }
    }

    public class ViewBetLevelForm : BackendPageBase
    {
        public ViewBetLevelForm(IWebDriver driver) : base(driver) {}
    }
}