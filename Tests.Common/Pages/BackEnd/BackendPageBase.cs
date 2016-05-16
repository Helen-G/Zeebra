using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages
{
    public abstract class BackendPageBase
    {
        protected IWebDriver _driver;

        protected BackendPageBase(IWebDriver driver)
        {
            _driver = driver;
        }

        public Uri Url
        {
            get { return new Uri(_driver.Url); }
        }
        
        protected virtual string GetPageUrl()
        {
            return "";
        }

        public BackendMenuBar Menu
        {
            get
            {
                return new BackendMenuBar(_driver);
            }
        }

        public BrandFilterControl BrandFilter
        {
            get
            {
                return new BrandFilterControl(_driver);
            }
        }

        public virtual void Initialize()
        {
            PageFactory.InitElements(_driver, this);
        }

        public virtual void NavigateToMemberWebsite()
        {
            var url = WebConfigurationManager.AppSettings["MemberWebsiteUrl"] + GetPageUrl();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();
            Initialize();
        }

        public virtual void NavigateToAdminWebsite()
        {
            var url = WebConfigurationManager.AppSettings["AdminWebsiteUrl"];
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();
            Initialize();
        }

        protected void SelectLicensee(By loadingComplete, By licenseeBy, string licensee)
        {
            //Wait for loading screen to finish.
            _driver.FindElementWait(loadingComplete);

            if (_driver.FindElements(licenseeBy).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var licenseeList = _driver.FindElementWait(licenseeBy);
                var licenseeField = new SelectElement(licenseeList);
                licenseeField.SelectByText(licensee);

                // Wait for dependent fields to transition. Currently, there is no reliable way to tell that the transition has finished.
                Thread.Sleep(300);
            }
        }

        protected void SelectLicenseeBrand(By loadingComplete, By licenseeBy, string licensee, By brandBy, string brand)
        {
            SelectLicensee(loadingComplete, licenseeBy, licensee);

            if (_driver.FindElements(brandBy).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var brandList = _driver.FindElementWait(brandBy);
                var brandField = new SelectElement(brandList);
                brandField.SelectByText(brand);

                // Wait for dependent fields to transition. Currently, there is no reliable way to tell that the transition has finished.
                Thread.Sleep(300);
            }
        }

        protected void SelectBrand(By loadingComplete, By brandBy, string brand)
        {
            if (_driver.FindElements(brandBy).Count(x => x.Displayed && x.Enabled) > 0)
            {
                var brandList = _driver.FindElementWait(brandBy);
                var brandField = new SelectElement(brandList);
                brandField.SelectByText(brand);

                // Wait for dependent fields to transition. Currently, there is no reliable way to tell that the transition has finished.
                Thread.Sleep(300);
            }
        }

        public void CloseTabWithTitleContains(string text)
        {
            var buttonXPath = string.Format("//span[contains(text(), '{0}')]/following-sibling::i", text);
            Click(By.XPath(buttonXPath));
        }

        public void CloseTab(string icon)
        {
            var iconXPath = string.Format("//span[text()='{0}']/following-sibling::i", icon);
            Click(By.XPath(iconXPath));
        }

        public void Click(By by)
        {
            var element = _driver.FindElementWait(by);
            element.Click();
        }

        public virtual string TabName
        {
            get { return _driver.FindElementValue(By.XPath("//div[contains(@class, 'nav-tabs-documents')]//ul[contains(@class, 'nav-tabs')]/li[contains(@class, 'active')]/a/span")); }
        }

        protected void ClearFieldsOnForm(string formXpath)
        {
            var clearButtonXpath = string.Format("//div[@data-view='{0}']//button[text()='Clear']", formXpath);
            var clearButton = _driver.FindElementWait(By.XPath(clearButtonXpath));
            clearButton.Click();
        }

        public IWebElement FindButton(By button)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            IWebElement element = null;
            wait.Until(d =>
            {
                element = _driver.FindElements(button).FirstOrDefault();
                return element != null;
            });
            return element;
        }

        public bool IsElementPresent(By elem)
        {
            IList<IWebElement> elems = _driver.FindElements(elem);
            return elems.Any();
        }

        public IWebElement FindActionButton(string buttonName, string contextXPath = null)
        {
            IWebElement buttonElement;
            contextXPath = contextXPath ?? "";
            try
            {
                buttonElement =
                    _driver.FindAnyElementsWait(By.XPath(string.Format("{0}//button[@name='{1}']", contextXPath, buttonName))).First();
            }
            catch (WebDriverTimeoutException)
            {
                var buttonNotFoundException = new RegoException(string.Format("Control button '{0}' not found.", buttonName));
                IWebElement moreButton;
                try
                {
                    moreButton =
                        _driver.FindElement(
                            By.XPath(string.Format("{0}//button[contains(@data-bind, 'click: toggleMoreExpanded')]", contextXPath)));
                }
                catch (WebDriverTimeoutException)
                {
                    throw buttonNotFoundException;
                }
                if (!moreButton.FindElements(By.ClassName("fa-caret-up")).Any())
                {
                    moreButton.Click();
                }
                try
                {
                    buttonElement =
                        _driver.FindAnyElementsWait(By.XPath(string.Format("{0}//li/a[@name='{1}']", contextXPath, buttonName))).First();
                }
                catch (WebDriverTimeoutException)
                {
                    throw buttonNotFoundException;
                }
            }
            return buttonElement;
        }

    }
}
