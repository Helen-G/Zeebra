using OpenQA.Selenium;
using AFT.RegoV2.Tests.Common.Extensions;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class NewEditRiskLevelForm : BackendPageBase
    {
        public NewEditRiskLevelForm(IWebDriver driver)
            : base(driver)
        { }

        public SubmittedRiskLevelForm NewSubmit(RiskLevelTestingDto dto)
        {
            SelectLicenseeBrand(By.XPath("//label[starts-with(@for, 'fraud-form-licensee-')]"),
                By.XPath("//select[starts-with(@id, 'fraud-form-licensee-')]"), dto.Licensee, By.XPath("//select[starts-with(@id, 'fraud-form-brand-')]"), dto.Brand);

            var elements = this._driver.FindElementsWait(By.CssSelector(".form-control"));

            var levelField = elements.GetElementByIdStartsWith("fraud-form-level-");
            levelField.Clear();
            levelField.SendKeys(dto.Level.ToString());

            var nameField = elements.GetElementByIdStartsWith("fraud-form-name-");
            nameField.Clear();
            nameField.SendKeys(dto.Name);

            var remarksField = elements.GetElementByIdStartsWith("fraud-form-description-");
            remarksField.Clear();
            remarksField.SendKeys(dto.Remarks);

            this.Click(By.XPath("//button[@type='submit']"));

            return new SubmittedRiskLevelForm(this._driver);
        }

        public SubmittedRiskLevelForm EditSubmit(string name, string remarks)
        {
            var elements = this._driver.FindElementsWait(By.CssSelector(".form-control"));

            var nameField = elements.GetElementByIdStartsWith("fraud-form-name-");
            nameField.Clear();
            nameField.SendKeys(name);

            var remarksField = elements.GetElementByIdStartsWith("fraud-form-description-");
            remarksField.Clear();
            remarksField.SendKeys(remarks);

            this.Click(By.XPath("//button[@type='submit']"));

            return new SubmittedRiskLevelForm(this._driver);
        }

    }

}
