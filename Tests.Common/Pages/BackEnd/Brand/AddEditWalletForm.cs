using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Tests.Selenium.Pages.BackEnd.Bonus
{
    public class AddEditWalletForm : BackendPageBase
    {
        public AddEditWalletForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedWalletTemplateForm Submit(string licensee, string brand, string[] productWallet=null)
        {
            SelectLicenseeBrand(By.XPath("//form[contains(@id, 'wallet-manager-form')]"),
                By.XPath("//select[contains(@id, 'licensee-select')]"), licensee,
                By.XPath("//select[contains(@id, 'brand-select')]"), brand);

            if (productWallet!=null)
            {
                AddProductWallet(productWallet);
            }

            var submitButton = _driver.FindElement(By.XPath("//div[@data-view='wallet/manager/edit']//button[text()='Save']"));
            submitButton.Click();

            return new SubmittedWalletTemplateForm(_driver);
        }

        public void AddProductWallet(string[]products)
        {
            var addWalletbutton = _driver.FindElementWait(By.XPath("//button[text()='Add product wallet']"));
            addWalletbutton.Click();
            var productWalletName = "PW" + TestDataGenerator.GetRandomString(5);
            var walletName = _driver.FindElementWait(By.XPath("//div[@data-bind='foreach: productWallets']//input"));
            walletName.SendKeys(productWalletName);
            products.ForEach(x => _driver.SelectFromMultiSelect("productsAssignControl", x));
        } 
    }
}