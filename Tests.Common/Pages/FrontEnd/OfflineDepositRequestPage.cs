using System;
using System.Diagnostics;
using System.Threading;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Infrastructure.DependencyResolution;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AFT.RegoV2.Shared;




namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class OfflineDepositRequestPage : FrontendPageBase
    {
        public OfflineDepositRequestPage(IWebDriver driver) : base(driver) {}

        public string ConfirmationMessage
        {
            get
            {
               return _driver.FindElementValue(By.XPath("//label[@data-bind='visible: offlineDepositSuccess']"));

            }
        }

        public string CheckBonusCode(string amount, string bonusCode)
        {
            var amountField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: amount, numeric: amount']"));
            amountField.SendKeys(amount);
            
            var bonusCodeField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: offlineDepositBonusCode']"));
            bonusCodeField.SendKeys(bonusCode);

            var checkButton = _driver.FindElementWait(By.XPath("//button[contains(@data-bind, 'click: checkODQualification')]"));
            checkButton.Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            return wait.Until(d =>
            {
                var confirmation = _driver.FindElementValue(By.XPath("//label[@data-bind='visible: ODQualificationSuccess']"));
                return confirmation;
            });
            
        }
        
        public void Submit(string amount, string playerRemark, string bankName = null)
        {
            if (bankName != null)
            {
                var bankField = _driver.FindElementWait(By.XPath("//select[@data-bind='value: bankAccount']"));
                var bankList = new SelectElement(bankField);
                bankList.SelectByText(bankName);
            }

            var amountField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: amount, numeric: amount']"));
            amountField.Clear();
            amountField.SendKeys(amount);

            var playerRemarkField = _driver.FindElementWait(By.XPath("//textarea[@data-bind='value: remarks']"));
            playerRemarkField.SendKeys(playerRemark);

            var submitButton = _driver.FindElementWait(By.XPath("//button[@data-bind='enable: !offlineDepositRequestInProgress()']"));
            submitButton.Click();
        }

    }
}