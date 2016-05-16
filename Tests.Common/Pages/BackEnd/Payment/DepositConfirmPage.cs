using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ConfirmOfflineDepositForm : BackendPageBase
    {
        public const string BaseXPath =
            "//div[contains(@class, 'tab-content') and not(contains(@style, 'display: none'))]//*[@data-view='player-manager/offline-deposit/confirm']";

        public ConfirmOfflineDepositForm(IWebDriver driver) : base(driver)
        {
        }

        public string Username
        {
            get
            {
                var username = _driver.FindElementValue(By.XPath(BaseXPath + "//p[contains(@data-bind, 'text: username')]"));
                return username;
            }
        }

        public string ReferenceCode
        {
            get
            {
                var referenceCode = _driver.FindElementValue(By.XPath(BaseXPath + "//p[contains(@data-bind, 'text: transactionNumber')]"));
                return referenceCode;
            }
        }

         public string BankAccountID
        {
            get { return _driver.FindElementValue(By.XPath("//label[text()='Bank Account ID']/following-sibling::div/p")); }
        }

         public string Amount
         {
             get { return _driver.FindElementValue(By.XPath("//label[text()='Amount ']/following-sibling::div/p")); }
         }

        public string Title
        {
            get { return _driver.FindElementValue(By.XPath("//span[text()='Deposit Confirm']")); }
        }

        private static string TempPlayerIdPath(string fileName)
        {
            var tempFileName = Path.GetTempFileName();
            var path = Path.ChangeExtension(tempFileName, "jpg");
            var resourceName = "AFT.RegoV2.Tests.Common.Resources." + fileName;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    var array = ms.ToArray();
                    File.WriteAllBytes(path, array);
                }
            }
            return path;
        }

        public SubmittedDepositConfirmForm SubmitValidDepositConfirm(DepositConfirmRegistrationData data, bool uploadImg = false)
        {
            var playerAccountNameElement = _driver.FindElementWait(By.XPath(BaseXPath + "//input[@name='playerAccountName']"));
            var inputAttempts = 0;
            const int attemptsLimit = 10;
            do
            {
                playerAccountNameElement.Clear();
                playerAccountNameElement.SendKeys(data.PlayerAccountName);
                inputAttempts++;
                if (inputAttempts == attemptsLimit)
                {
                    throw new RegoException("Unable to input Player Account Name");
                }
            } 
            while (playerAccountNameElement.GetAttribute("value") != data.PlayerAccountName);

            _playerAccountNumber.SendKeys(data.PlayerAccountNumber);
            _bankReferenceNumber.SendKeys(data.BankReferenceNumber);
            var amountField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[@name='amount']"));
            amountField.Clear();
            amountField.SendKeys(data.Amount.ToString(CultureInfo.InvariantCulture));
            var transfertypeList = _driver.FindElementWait(By.XPath(BaseXPath + "//*[@name='transferType']"));
            var transfertype = new SelectElement(transfertypeList);
            transfertype.SelectByText(data.TransferType);
            var offlineDepositTypeList = _driver.FindElementWait(By.XPath(BaseXPath + "//*[@name='offlineDepositType']"));
            var offlineDepositType = new SelectElement(offlineDepositTypeList);
            offlineDepositType.SelectByText(data.OfflineDepositType);
            _remark.SendKeys(data.Remarks);

            if (uploadImg)
            {
                IdFrontImageInput.SendKeys(TempPlayerIdPath("player-front-id.jpg"));
                IdBackImageInput.SendKeys(TempPlayerIdPath("player-back-id.jpg"));
            }

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(x => _submitButton.Enabled);
            _driver.ScrollPage(0, 1200);
            _submitButton.Click();
            var tab = new SubmittedDepositConfirmForm(_driver);
            tab.Initialize();
            return tab;
        }

        
#pragma warning disable 649
        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[@name='playerAccountNumber']")]
        private IWebElement _playerAccountNumber;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//input[@name='referenceNumber']")]
        private IWebElement _bankReferenceNumber;

        [FindsBy(How = How.XPath, Using = BaseXPath +"//*[contains(@id, 'deposit-confirm-remark')]")]
        private IWebElement _remark;

        [FindsBy(How = How.XPath, Using = BaseXPath + "//button[text() = 'Submit']")]
        private IWebElement _submitButton;

        [FindsBy(How = How.XPath, Using = "//span[@data-bind='validationMessage: idFrontImage']")]
        public IWebElement IdFrontImageValidationMessage { get; set; }

        [FindsBy(How = How.XPath, Using = "//span[@data-bind='validationMessage: idBackImage']")]
        public IWebElement IdBackImageValidationMessage { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId1']/following-sibling::span[@class='ace-file-container']")]
        public IWebElement IdFrontImage { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId2']/following-sibling::span[@class='ace-file-container']")]
        public IWebElement IdBackImage { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId1']")]
        public IWebElement IdFrontImageInput { get; set; }

        [FindsBy(How = How.XPath, Using = "//input[@name='uploadId2']")]
        public IWebElement IdBackImageInput { get; set; }
#pragma warning restore 649

    }

    
}