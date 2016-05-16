using System;
using System.Collections.Generic;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class IpRegulationManagerPage : BackendPageBase
    {
        public IpRegulationManagerPage(IWebDriver driver)
            : base(driver)
        {
        }

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "ipaddress-search", "search-button");
            }
        }

        public NewIpRegulationForm OpenNewIpRegulationForm()
        {
            var newIpRegulationButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: openAddIpRegulationTab')]"));
            newIpRegulationButton.Click();
            return new NewIpRegulationForm(_driver);
        }

        public void DeleteIpRegulation(string ipAddress)
        {
            Grid.SelectRecord(ipAddress);
            var deleteButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: deleteIpRegulation')]"));
            deleteButton.Click();
            var yesButton = _driver.FindElementWait(By.XPath("//div[contains(@class, 'modal-content')]//button[text()='Yes']"));
            yesButton.Click();
            var closeButton = _driver.FindElementWait(By.XPath("//div[contains(@class, 'modal-content')]//button[text()='Close']"));
            closeButton.Click();
            deleteButton.Click();
        }

        public EditIpRegulationForm OpenEditIpRegulationForm(string ipAddress)
        {
            Grid.SelectRecord(ipAddress);
            var deleteButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: openEditIpRegulationTab')]"));
            deleteButton.Click();
            var form = new EditIpRegulationForm(_driver);
            return form;
        }

        public void Search()
        {
            var searchButton = _driver.FindLastElementWait(By.XPath("//button[text()='Search']"));
            searchButton.Click();
        }

        public bool CheckIfNoRecordsDisplayed()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            try
            {
                _driver.FindElementWait(By.XPath("//div[contains(text(), 'No records to view')]"));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

    }

    public class IpRegulationData
    {
        public string Licensee { get; set; }
        public string Brand { get; set; }
        public string IpAddress { get; set; }
        public bool AdvancedSettings { get; set; }
        public string MultipleIpAddress { get; set; }
        public string Description { get; set; }
        public string Restriction { get; set; }
        public bool ApplyToBrand { get; set; }
        public bool ApplyToBackend { get; set; }
        public string BlockingType { get; set; }
        public string RedirectUrl { get; set; }

        public string GetRestriction()
        {
            switch (Restriction)
            {
                case "Allow Access":
                    return "Accessible";
                case "Block":
                    return "Block";
                default:
                    throw new RegoException("Unrecognized restriction type");
            }
        }

        public string AppliedTo
        {
            get
            {
                var applied = new List<string>();
                if (ApplyToBrand)
                    applied.Add("Brand");
                if (ApplyToBackend)
                    applied.Add("Backend");

                return string.Join(", ", applied);
            }
        }
    }
}
