using OpenQA.Selenium;


namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{    /// <summary>
    /// Represents Fraud -> Auto Verification Configuration page 
    /// </summary>
    public class AutoVerificationConfigurationPage: BackendPageBase
    {
        internal static readonly By NewButtonBy = By.XPath("//button[contains(@class,'btn') and contains(@data-bind,'add')]");
        internal static readonly By EditButtonBy = By.XPath("//button[contains(@class,'btn') and contains(@data-bind,'openEditTab')]");

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "verification-manager-name-search", "verification-manager-search-button");
            }
        }

        public NewAutoVerificationConfigurationForm OpenNewAutoVerificationForm()
        {
            Click(NewButtonBy);
            var page = new NewAutoVerificationConfigurationForm(_driver);
            page.Initialize();
            return page;
        }

    public void SelectAvcRecord(AutoVerificationConfigurationData avcData)
    {
        Grid.FilterGrid(avcData.Brand);
        var recordXPath =
            string.Format(
                "//table[@id='verification-manager-list']//tr[contains(., '{0}') and contains(., '{1}') and contains(., '{2}') and contains(., '{3}')]",
                avcData.Licensee, avcData.Brand, avcData.Currency, avcData.VipLevel);

        Click(By.XPath(recordXPath));
    }

    public EditAutoVerificationConfigurationForm OpenEditAutoVerificationConfigurationForm(AutoVerificationConfigurationData avcData)
        {
            SelectAvcRecord(avcData);
            Click(EditButtonBy);
            var form = new EditAutoVerificationConfigurationForm(_driver);
            form.Initialize();
            return form;
        }

        public AutoVerificationConfigurationPage(IWebDriver driver)
            : base(driver)
        { }
    }
}
