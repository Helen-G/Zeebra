using System;
using System.Web.Configuration;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace AFT.RegoV2.Tests.Common.Pages
{
    public abstract class FrontendPageBase
    {
        protected IWebDriver _driver;

        protected FrontendPageBase(IWebDriver driver)
        {
            _driver = driver;
        }

        public Uri Url
        {
            get { return new Uri(_driver.Url); }
        }
        
        public bool IsCurrentPage 
        {
            get { return GetPageUrl() == _driver.Url; }
        }

        protected virtual string GetPageUrl()
        {
            return "";
        }

        public FrontendMenuBar Menu
        {
            get
            {
                return new FrontendMenuBar(_driver);
            }
        }

        public virtual void Initialize()
        {
            PageFactory.InitElements(_driver, this);
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

        public virtual void NavigateToMemberWebsite()
        {
            var url = WebConfigurationManager.AppSettings["MemberWebsiteUrl"] + GetPageUrl();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().Refresh();
            Initialize();
        }
        


    }
}
