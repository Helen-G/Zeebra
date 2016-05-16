using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Configuration;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace AFT.RegoV2.Tests.Selenium
{
    [Category("Selenium")]
    internal abstract class SeleniumBase : TestsBase
    {
        protected IWebDriver    _driver;

        public override void BeforeAll()
        {
            base.BeforeAll();

            _driver = CreateFireFoxWebDriver();
            _driver.Url = GetWebsiteUrl();
            _driver.Manage().Window.Size = new Size(1500, 900);
        }

        static IWebDriver CreateChromeWebDriver()
        {
            //use chromium if it's found in predefined folder
            var options = new ChromeOptions();
            const string chromiumFilePath = @"C:\chromium\chrome.exe";
            if (File.Exists(chromiumFilePath))
            {
                options.BinaryLocation = chromiumFilePath;
                const string chromeDriverDirectory = @"C:\chromium";
                return new ChromeDriver(chromeDriverDirectory, options);
            }
            return new ChromeDriver(options);
        }

        static IWebDriver CreateFireFoxWebDriver()
        {
            return new FirefoxDriver();
        }

        public override void BeforeAllFailed(Exception ex)
        {
            base.BeforeAllFailed(ex);
            SaveScreenshot();
            QuitWebDriver();
        }

        public override void AfterAll()
        {
            base.AfterAll();
            QuitWebDriver();
        }

        public override void BeforeEachFailed(Exception ex)
        {
            base.BeforeEachFailed(ex);
            SaveScreenshot();
        }

        public override void AfterEachFailed()
        {
            base.AfterEachFailed();
            SaveScreenshot();
        }

        public void SaveScreenshot()
        {
            if (_driver == null)
                return;
            var path = WebConfigurationManager.AppSettings["ScreenshotsPath"];
            var fileName = string.Format("screenshot-{0:yyyy-MM-dd_hh-mm}.{1}", DateTime.Now, "png");
            var fullPath = Path.Combine(path, fileName);

            Screenshot screenshot = ((ITakesScreenshot) _driver).GetScreenshot();
            Thread.Sleep(500);
            screenshot.SaveAsFile(fullPath, System.Drawing.Imaging.ImageFormat.Png);
            Thread.Sleep(500);
        }

        protected abstract string GetWebsiteUrl();

        protected void QuitWebDriver()
        {
            if (_driver == null) return;

            var exceptionThrown = false;
            var retries = 0;
            do
            {
                try
                {
                    retries++;
                    _driver.Quit();
                }
                catch (Exception exception)
                {
                    exceptionThrown = true;
                    SaveScreenshot();
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }

            } while (exceptionThrown && retries <= 3);
        }
    }

    abstract class SeleniumBaseForGameWebsite : SeleniumBase
    {
        protected override string GetWebsiteUrl()
        {
            return WebConfigurationManager.AppSettings["MockGameWebsite"];
        }
    }

    public class CategorySmoke : CategoryAttribute
    {
        public CategorySmoke() : base("Smoke") { }
    }
}
