using System.Collections.Generic;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class SubmittedRiskLevelForm : BackendPageBase
    {
        private IEnumerable<IWebElement> _elements;

        public static readonly string Created = "The FraudRiskLevel has been successfully created";
        public static readonly string Updated = "The FraudRiskLevel has been successfully updated";

        public SubmittedRiskLevelForm(IWebDriver driver)
            : base(driver)
        { }

        public string ConfirmationMessage
        {
            get { return this._driver.FindElementValue(By.XPath("//div[contains(@class, 'alert')]")); }
        }

        public void PreReadValue()
        {
            if (this._elements == null)
            {
                this._elements = this._driver.FindElementsWait(By.CssSelector(".form-control-static"));
            }
        }

        public string Name
        {
            get
            {
                this.PreReadValue();
                return this._elements.GetFieldValue("name");
            }
        }

        public int Level
        {
            get
            {
                this.PreReadValue();
                var value = this._elements.GetFieldValue("level");
                if (!string.IsNullOrEmpty(value))
                {
                    return System.Int32.Parse(value);
                }
                return 0;
            }
        }

        public string Remarks
        {
            get
            {
                this.PreReadValue();
                return this._elements.GetFieldValue("description");
            }
        }

        public void CloseNewForm()
        {
            this.CloseTab("New Fraud");
        }

        public void CloseEditForm()
        {
            this.CloseTab("Edit Fraud");
        }

    }
}
