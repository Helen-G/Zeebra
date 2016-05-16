using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class EditBankForm : BackendPageBase
    {
        public EditBankForm(IWebDriver driver) : base(driver)
        {
        }

        public SubmittedBankForm Submit(string licensee, string brand, string bankName)
        {
            SelectLicenseeBrand(By.XPath("//label[contains(@for, 'bank-licensee')]"),
                By.XPath("//select[contains(@id, 'bank-licensee')]"), licensee, By.XPath("//select[contains(@id, 'bank-brand')]"), brand);

            var bankNameField = _driver.FindElementWait(By.XPath("//input[contains(@id, 'bank-name')]"));
            bankNameField.Clear();
            bankNameField.SendKeys(bankName);
            var remarksField = _driver.FindElementWait(By.XPath("//textarea[contains(@id, 'bank-remark')]"));
            remarksField.SendKeys("new bank");
            var saveButton = _driver.FindElementWait(By.XPath("//button[text()='Save']"));
            saveButton.Click();
            var form = new SubmittedBankForm(_driver);
            return form;
        }
    }
} 