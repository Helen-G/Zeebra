using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports
{
    public class PaymentReportsPage : ReportsPageBase
    {
        public PaymentReportsPage(IWebDriver driver) : base(driver) { }

        public DepositReportPage OpenDepositReport()
        {
            OpenReport("Deposit Report");
            return new DepositReportPage(_driver);
        }
    }

    
    public class DepositReportPage : ReportPageBase
    {
        public DepositReportPage(IWebDriver driver) : base(driver)
        {
            ReportViewPath = "payment/deposit";
        }
    }
}