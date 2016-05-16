using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports
{
    public class PlayerReportsPage : ReportsPageBase
    {
        public PlayerReportsPage(IWebDriver driver) : base(driver) {}

        public PlayerReportPage OpenPlayerReport()
        {
            OpenReport("Player Report");
            return new PlayerReportPage(_driver);
        }

        public PlayerBetHistoryReportPage OpenPlayerBetHistoryReport()
        {
            OpenReport("Player Bet History Report");
            return new PlayerBetHistoryReportPage(_driver);
        }
    }

    
    public class PlayerReportPage : ReportPageBase
    {
        public PlayerReportPage(IWebDriver driver) : base(driver)
        {
            ReportViewPath = "player/player";
        }
    }
 

    public class PlayerBetHistoryReportPage : ReportPageBase
    {
        public PlayerBetHistoryReportPage(IWebDriver driver) : base(driver)
        {
            ReportViewPath = "player/player-bet-history";
        }
    }
}