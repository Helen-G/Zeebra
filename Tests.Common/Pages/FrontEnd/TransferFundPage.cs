using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.FrontEnd
{
    public class TransferFundPage : FrontendPageBase
    {
        public TransferFundPage(IWebDriver driver) : base(driver) {}

        public void TryToMakeInvalidTransferFundRequest(TransferFundType transferType, string productWallet, string amount)
        {
            var transferTypeField = _driver.FindElementWait(By.XPath("//select[@data-bind='value: transferFundType']"));
            var transferFundType = new SelectElement(transferTypeField);

            switch (transferType)
            {
                case TransferFundType.FundIn:
                    {
                        transferFundType.SelectByValue("FundIn");
                        break;
                    }

                case TransferFundType.FundOut:
                    {
                        transferFundType.SelectByValue("FundOut");
                        break;
                    }

                default:
                    {
                        transferFundType.SelectByValue("FundIn");
                        break;
                    }
            }

            var productWalletField = _driver.FindElementWait(By.XPath("//select[@data-bind='value: fundInWallet']"));
            var productWalletList = new SelectElement(productWalletField);
            productWalletList.SelectByText(productWallet);

            var transferAmount = _driver.FindElementWait(By.XPath("//input[@data-bind='value: fundInAmount']"));
            transferAmount.Clear();
            transferAmount.SendKeys(amount);

            var submitButton =
                _driver.FindElementWait(By.XPath("//button[@data-bind='enable: !fundInRequestInProgress()']"));
            submitButton.Click();
        }

        public string ValidationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//label[contains(@data-bind, 'text: fundInErrors')]")); }
        }

        public void FundIn(decimal amount, string bonusCode = null)
        {
            var amountField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: fundInAmount']"));
            //amountField.Clear();
            amountField.SendKeys(amount.ToString());

            if (bonusCode != null)
            {
                var bonusCodeField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: fundInBonusCode']"));
                bonusCodeField.SendKeys(bonusCode);
            }
            var submitBtn = _driver.FindElementWait(By.XPath("//button[@data-bind='enable: !fundInRequestInProgress()']"));
            submitBtn.Click();
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//label[@data-bind='visible: fundInSuccess']")); }
        }

        public string Balance
        {
            get
            {
                return _driver.FindElementValue(By.XPath("//span[@data-bind='text: walletBalance']"));
            }
        }

        public void FundOut(decimal amount)
        {
            var transferTypeField = _driver.FindElementWait(By.XPath("//select[@data-bind='value: transferFundType']"));
            var transferFundType = new SelectElement(transferTypeField);
            transferFundType.SelectByText("Fund Out");

            var amountField = _driver.FindElementWait(By.XPath("//input[@data-bind='value: fundInAmount']"));
            amountField.Clear();
            amountField.SendKeys(amount.ToString());

            var submitButton =
                _driver.FindElementWait(By.XPath("//button[@data-bind='enable: !fundInRequestInProgress()']"));
            submitButton.Click();
        }


#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = "//input[@data-bind='value: fundInAmount']")] 
        public IWebElement _amountField;

        [FindsBy(How = How.XPath, Using = "//select[@data-bind='value: transferFundType']")] 
        public IWebElement _transferTypeField;

        [FindsBy(How = How.XPath, Using = "//button[@data-bind='enable: !fundInRequestInProgress()']")] 
        public IWebElement _submitButton;
#pragma warning restore 649
    }

    public class SubmittedTransferFundForm : FrontendPageBase
    {
        public SubmittedTransferFundForm(IWebDriver driver) : base(driver)
        {
        }
    }
}