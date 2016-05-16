using System;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public class AddEditBonusForm: BackendPageBase
    {
        public AddEditBonusForm(IWebDriver driver) : base(driver) {}
        private const string FormXPath = "//div[@data-view='bonus/bonus-manager/add-edit-bonus']";

        public SubmittedBonusForm Submit(string bonusName, string bonusCode, string bonusTemplateName, int numberOfdaysToClaimBonus)
        {
            SelectBonusTemplate(bonusTemplateName);
            SetNameAndCode(bonusName, bonusCode);

            var dateRangeField = _driver.FindElementWait(By.XPath("//input[contains(@data-bind, 'dateRange: true')]"));
            dateRangeField.Click();
            var fromDate =
                _driver.FindElementWait(
                    By.XPath("//div[contains(@style, 'display: block')]//input[@name='daterangepicker_start']"));
            fromDate.Clear();
            fromDate.SendKeys("2014/10/20");
            var toDate =
                _driver.FindElementWait(
                    By.XPath("//div[contains(@style, 'display: block')]//input[@name='daterangepicker_end']"));
            toDate.Clear();
            toDate.SendKeys("2015/10/20");

            var applyButton = _driver.FindElementWait(By.XPath("//div[@class='daterangepicker dropdown-menu show-calendar opensright']//button[text()='Apply']"));
            applyButton.Click();

            var daysToClaimField = _driver.FindElementWait(By.XPath(FormXPath + "//input[contains(@data-bind, 'value: DaysToClaim')]"));
            daysToClaimField.SendKeys(numberOfdaysToClaimBonus.ToString());

            return Submit();
        }

        public SubmittedBonusForm Submit()
        {
            var submitButton = _driver.FindElementWait(By.XPath(FormXPath + "//button[@data-bind='click: submit']"));
            submitButton.Click();

            return new SubmittedBonusForm(_driver);
        }

        public void SetNameAndCode(string bonusName, string bonusCode)
        {
            var bonusNameField = _driver.FindElementWait(By.XPath(FormXPath + "//input[@data-bind='value: Name']"));
            bonusNameField.SendKeys(bonusName);

            var bonusCodeField = _driver.FindElementWait(By.XPath(FormXPath + "//input[@data-bind='value: Code']"));
            bonusCodeField.SendKeys(bonusCode);
        }

        private void SelectBonusTemplate(string bonusTemplateName)
        {
            var templatesList = _driver.FindElementWait(By.XPath("//select[@data-bind[contains(., 'value: TemplateId')]]"));
            var bonusTempateValue = new SelectElement(templatesList);
            bonusTempateValue.SelectByText(bonusTemplateName);
        }
    }
}