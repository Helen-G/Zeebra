using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;

namespace AFT.RegoV2.Tests.Common.Extensions
{
    public static class WebElementExtensions
    {
        /// <summary>
        /// When SeleniumDriver's window looses it's focus, IWebElement.SendKeys method is not initiating onchange event.
        /// This method always initiates onchange event after sending text to the browser window
        /// </summary>
        /// <param name="element"></param>
        /// <param name="text"></param>
        public static void SendKeysWithOnChangeEvent(this IWebElement element, string text)
        {
            element.SendKeys(text);

            var selector = GenerateCssSelectorOrNull(element);
            if (selector == null)
                throw new ApplicationException(string.Format("Unable to generate xpath for the IWebElement {0}", element.TagName));

            var driver = ((IWrapsDriver)(((IWrapsElement)(element)).WrappedElement)).WrappedDriver;
            ((IJavaScriptExecutor)driver).ExecuteScript("$(arguments[0]).change(); return true;", selector);
        }

        private static String GenerateCssSelectorOrNull(IWebElement childElement, string current = null)
        {
            if (current == null) current = string.Empty;

            var childTag = childElement.TagName;
            if (childTag == "html")
            {
                return "html" + current;
            }
            var parentElement = childElement.FindElement(By.XPath(".."));
            var childrenElements = parentElement.FindElements(By.XPath("*"));
            
            for (int i = 0; i < childrenElements.Count(); i++)
            {
                var childrenElement = childrenElements[i];
                if (childElement.Equals(childrenElement))
                {
                    var nthChildIndex = i + 1;
                    return GenerateCssSelectorOrNull(parentElement, " > *:nth-child(" + nthChildIndex + ")" + current);
                }
            }
            return null;
        }
    }
}