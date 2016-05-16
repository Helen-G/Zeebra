using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewProductForm : BackendPageBase
    {
        public NewProductForm(IWebDriver driver) : base(driver) { }

        public ViewProductForm Submit(string name, string type, string authMethod)
        {
            var productName = _driver.FindElementWait(By.XPath("//div[@data-bind='with: name']/div/input"));
            productName.SendKeys(name);

            var productTypeList = _driver.FindElementWait(By.XPath("//div[@data-bind='with: category']/div/select"));
            var productType = new SelectElement(productTypeList);
            productType.SelectByText(type);

            var authenticationMethodList = _driver.FindElementWait(By.XPath("//div[@data-bind='with: authentication']/div/select"));
            var authenticationMethod = new SelectElement(authenticationMethodList);
            authenticationMethod.SelectByText(authMethod);

            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var submittedForm = new ViewProductForm(_driver);
            return submittedForm;
        }
    }

    public class ViewProductForm : BackendPageBase
    {
        public ViewProductForm(IWebDriver driver) : base(driver) { }
    }
}