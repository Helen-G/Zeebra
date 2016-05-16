using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Extensions
{
    public static class WebDriverExtensions
    {
        public static void WaitForJavaScript(this IWebDriver driver)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(45));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript("return (typeof jQuery == 'undefined') || jQuery.active == 0"));
        }

        public static IWebElement FindElementWait(this IWebDriver driver, By by)
        {
            var elements = FindElementsWait(driver, @by);

            return elements.First();
        }

        public static IWebElement FindLastElementWait(this IWebDriver driver, By by)
        {
            var elements = FindElementsWait(driver, @by);

            return elements.Last();
        }

        public static IEnumerable<IWebElement> FindElementsWait(this IWebDriver driver, By by)
        {
            var elements = FindAnyElementsWait(driver, @by, x => x.Displayed && x.Enabled);

            return elements;
        }

        public static IWebElement FindAnyElementWait(this IWebDriver driver, By by, Func<IWebElement, bool> predicate = null)
        {
            var elements = FindAnyElementsWait(driver, by, predicate);

            return elements.First();
        }

        public static IEnumerable<IWebElement> FindAnyElementsWait(this IWebDriver driver, By by, Func<IWebElement, bool> predicate = null)
        {
            driver.WaitForJavaScript();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            IEnumerable<IWebElement> foundElements = null;

            const int maxAttemptCount = 5;
            var attemptCount = 0;
            while (true)
            {
                try
                {
                    wait.Until(d =>
                    {
                        foundElements = driver.FindElements(@by);

                        if (predicate != null)
                            foundElements = foundElements.Where(predicate);

                        return foundElements.Any();
                    });
                    break;
                }
                catch (StaleElementReferenceException)
                {
                    attemptCount++;
                    if (attemptCount < maxAttemptCount)
                    {
                        continue;
                    }
                    throw;
                }
            }

            return foundElements;
        }

        public static string FindElementValue(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            IWebElement element = null;

            wait.Until(d =>
            {
                try
                {
                    element = driver.FindElements(@by).FirstOrDefault(x => x.Displayed);
                }
                catch (StaleElementReferenceException)
                {
                    //there may be some page/control refreshes happening during this time
                    //so it's totally fine to ignore this specific exception if it happens
                } 

                return element != null && element.Text != string.Empty;
            });

            return element.Text;
        }

        public static IWebElement FindElementScroll(this IWebDriver driver, By by)
        {
            var element = FindElementWait(driver, @by);
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView();", element);
            return element;
        }

        public static void SelectRecordInGridExtendFilter(this IWebDriver driver, string column, string condition, string value)
        {
            driver.TypeFilterCriterion(column, condition, value);
            driver.GenerateReport();
            var userRecord = String.Format("//td[text() =\"{0}\"]", value);
            var firstCell = driver.FindElementWait(By.XPath(userRecord));
            firstCell.Click();
        }

        public static void OpenExtendedFilter(this IWebDriver driver)
        {
            var showFilterButton = driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: showFilter')]|//i[contains(@data-bind, 'click: showFilter')]"));
            showFilterButton.Click();
        }

        public static void TypeFilterCriterion(this IWebDriver driver, string column, string condition, string value)
        {
            var columnDropDownXPath = By.XPath("//select[contains(@data-bind, 'value: filterField')]");

            if (driver.FindElements(columnDropDownXPath).Count(x => x.Displayed) == 0)
            {
                OpenExtendedFilter(driver);
            }
            var columnDropDown = driver.FindElementWait(columnDropDownXPath);
            var columnField = new SelectElement(columnDropDown);
            columnField.SelectByText(column);

            var conditionDropDown = driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'value: condition')]"));
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(x => conditionDropDown.Displayed);
            var conditionField = new SelectElement(conditionDropDown);
            conditionField.SelectByText(condition);

            var valueField = driver.FindAnyElementWait(By.XPath("//input[@name='value']"));
            if (valueField.Displayed)
            {
                valueField.Clear();
                valueField.SendKeys(value);
            }
            else
            {
                var valueDropDown = driver.FindElementWait(By.XPath("//select[contains(@data-bind, 'value: selectedValue')]"));
                new SelectElement(valueDropDown).SelectByText(value);
            }
        }

        public static void GenerateReport(this IWebDriver driver)
        {
            var searchButton = driver.FindLastElementWait(By.XPath("//button[text()='Search']|//button[@type='submit']"));
            searchButton.Click();
        }

        public static Uri Uri(this IWebDriver driver)
        {
            return new Uri(driver.Url);
        }

        public static void ScrollPage(this IWebDriver driver, int x, int y)
        {
            var coordinatesToScroll = String.Format("scroll('{0}', '{1}')", x, y);
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
            jse.ExecuteScript(coordinatesToScroll);
        }

        public static void SelectFromMultiSelect(this IWebDriver driver, string controlName, string optionToSelect)
        {
            var controlPath = String.Format("//div[contains(@data-bind, 'with: {0}')]", controlName);
            var control = driver.FindElementWait(By.XPath(controlPath));
            var list = control.FindElement(By.XPath(".//select[contains(@data-bind, 'options: availableItems')]"));
            var listField = new SelectElement(list);
            listField.SelectByText(optionToSelect);
            var assignButton = control.FindElements(By.XPath(controlPath + "//button[contains(@data-bind, 'click: assign')]")).Last();
            assignButton.Click();
        }

        public static void FindElementClick(this IWebDriver driver, string element)
        {
            driver.FindElementWait(By.XPath(element)).Click();
        }

        public static IWebElement UpdateTextField(this IWebDriver driver, By by, string text, bool clearCurrentText = false)
        {
            var textField = driver.FindElementWait(@by);

            if (clearCurrentText) textField.Clear();

            textField.SendKeys(text);

            return textField;
        }

        public static IWebElement UpdateCheckboxState(this IWebDriver driver, By by, bool targetStateIsChecked)
        {
            var checkBoxField = driver.FindElementWait(@by);

            if (checkBoxField.Selected && !targetStateIsChecked || !checkBoxField.Selected && targetStateIsChecked)
                checkBoxField.Click();

            return checkBoxField;
        }

        public static IWebElement GetElementByIdStartsWith(this IEnumerable<IWebElement> elements, string id)
        {
            return elements.GetElementByAttributeStartsWith("id", id);
        }

        public static IWebElement GetElementByAttributeStartsWith(this IEnumerable<IWebElement> elements, string attributeName, string attributeValue)
        {
            return elements.FirstOrDefault(x => x.GetAttribute(attributeName).StartsWith(attributeValue));
        }

        public static string GetFieldValue(this IEnumerable<IWebElement> elements, string fieldName)
        {
            var element = elements.GetElementByAttributeStartsWith("data-bind", "text: fields." + fieldName);
            if (element != null)
            {
                return element.Text;
            }
            return null;
        }
    }
}