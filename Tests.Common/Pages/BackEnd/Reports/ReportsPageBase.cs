using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports
{
    public class ReportsPageBase : BackendPageBase
    {
        protected const string GridXPath = "//div[contains(@data-view, 'reports/list')]";

        public ReportsPageBase(IWebDriver driver) : base(driver) { }

        protected void OpenReport(string title)
        {
            var reportType = _driver.FindElementWait(By.XPath(String.Format("//td[@title='{0}']", title)));
            reportType.Click();
            var viewButton = FindActionButton("open", GridXPath);
            viewButton.Click();
        }

    }

    public class ReportPageBase : BackendPageBase
    {
        public ReportPageBase(IWebDriver driver) : base(driver) { }

        // Should be assigned in descenant class.
        protected string ReportViewPath;

        private string ReportXPath
        {
            get { return String.Format("//div[@data-view='reports/{0}']", ReportViewPath); }
        }

        private string PlayerRowsXPath
        {
            get { return ReportXPath + "//table[@class='ui-jqgrid-btable']//tr"; }
        }

        public bool CheckIfAnyReportDataDisplayed()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(x => _driver.FindElements(By.XPath(PlayerRowsXPath + "[2]/td[1]")).Any());
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        public void WaitUntilReportDataIsEmpty()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(x => !_driver.FindElements(By.XPath(PlayerRowsXPath + "[2]/td[1]")).Any());
        }
    }
}
