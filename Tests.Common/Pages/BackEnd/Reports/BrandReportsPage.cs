using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Reports
{
    public class BrandReportsPage : ReportsPageBase
    {
        public BrandReportsPage(IWebDriver driver) : base(driver) { }

        public BrandReportPage OpenBrandReport()
        {
            OpenReport("Brand Report");
            return new BrandReportPage(_driver);
        }

        public LicenseeReportPage OpenLicenseeReport()
        {
            OpenReport("Licensee Report");
            return new LicenseeReportPage(_driver);
        }

        public LanguageReportPage OpenLanguageReport()
        {
            OpenReport("Language Report");
            return new LanguageReportPage(_driver);
        }

        public VipLevelReportPage OpenVipLevelReport()
        {
            OpenReport("VIP Level Report");
            return new VipLevelReportPage(_driver);
        }
    }

    
    public class BrandReportPage : ReportPageBase
    {
        public BrandReportPage(IWebDriver driver) : base(driver)
        {
            ReportViewPath = "brand/brand";
        }
    }

    public class LicenseeReportPage : ReportPageBase
    {
        public LicenseeReportPage(IWebDriver driver)
            : base(driver)
        {
            ReportViewPath = "brand/licensee";
        }
    }

    public class LanguageReportPage : ReportPageBase
    {
        public LanguageReportPage(IWebDriver driver)
            : base(driver)
        {
            ReportViewPath = "brand/language";
        }
    }

    public class VipLevelReportPage : ReportPageBase
    {
        public VipLevelReportPage(IWebDriver driver)
            : base(driver)
        {
            ReportViewPath = "brand/vipLevel";
        }
    }
}