using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class SubmittedPaymentSettingsForm : BackendPageBase
    {
        public SubmittedPaymentSettingsForm(IWebDriver driver)
            : base(driver)
        {
        }

        public string ConfirmationMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-success']")); }
        }

        public string AlertMessage
        {
            get { return _driver.FindElementValue(By.XPath("//div[@class='alert alert-danger']")); }
        }

        public string Licensee
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.licensee']/p")
                            );
            }
        }

        public string Brand
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.brand']/p")
                            );
            }
        }

        public string Currency
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.currency']/p")
                            );
            }
        }

        public string PaymentType
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.paymentType']/p")
                            );
            }
        }

        public string VipLevel
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.vipLevel']/p")
                            );
            }
        }

        public string PaymentMethod
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//div[@data-view='payments/settings/details']//div[@data-bind='with: form.fields.paymentMethod']/p")
                            );
            }
        }


        public string MinAmountPerTransaction
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.minAmountPerTransaction')]")
                            );
            }
        }

        public string MaxAmountPerTransaction
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.maxAmountPerTransaction')]")
                            );
            }
        }

        public string MaxAmountPerDay
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.maxAmountPerDay')]")
                            );
            }
        }

        public string MaxTransactionsPerDay
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.maxTransactionPerDay')]")
                            );
            }
        }

        public string MaxTransactionsPerWeek
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.maxTransactionPerWeek')]")
                            );
            }
        }

        public string MaxTransactionsPerMonth
        {
            get
            {
                return
                    _driver.FindElementValue(
                        By.XPath(
                            "//p[contains(@data-bind, 'fields.maxTransactionPerMonth')]")
                            );
            }
        }


    }
}