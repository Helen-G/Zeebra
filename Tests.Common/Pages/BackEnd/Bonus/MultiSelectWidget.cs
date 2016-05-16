using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    internal class MultiSelectWidget
    {
        private readonly IWebElement _control;

        public MultiSelectWidget(IWebDriver driver, By widgetLocator)
        {
            _control = driver.FindElementWait(widgetLocator);
        }

        public void SelectFromMultiSelect(string optionToSelect)
        {
            var checkbox = _control.FindElement(By.XPath(string.Format(".//ul[contains(@data-bind, 'foreach: availableItems')]/li/label[span[text()='{0}']]/input", optionToSelect)));
            if (!checkbox.Selected)
            {
                checkbox.Click();
            }
            var assignButton = _control.FindElement(By.XPath(".//button[contains(@data-bind, 'click: assign')]"));
            assignButton.Click();
        }

        public void DeselectFromMultiSelect(string optionToSelect)
        {
            var checkbox = _control.FindElement(By.XPath(string.Format(".//ul[contains(@data-bind, 'foreach: assignedItems')]/li/label[span[text()='{0}']]/input", optionToSelect)));
            if (!checkbox.Selected)
            {
                checkbox.Click();
            }
            var unassignButton = _control.FindElement(By.XPath(".//button[contains(@data-bind, 'click: unassign')]"));
            unassignButton.Click();
        }
    }
}
