using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class NewVipLevelForm : BackendPageBase
    {
        public By LoadingCompleteBy { get { return By.XPath("//label[contains(@for, 'vip-level-licensee')]"); } }
        public By LicenseeSelectBy { get { return By.XPath("//select[contains(@id, 'vip-level-licensee')]"); } }
        public By BrandSelectBy { get { return By.XPath("//select[contains(@id, 'vip-level-brand')]"); } }
        public By DefaultForNewPlayersCheckboxBy { get { return By.XPath("//input[contains(@id, 'vip-level-is-default')]"); } }
        public By CodeFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-code')]"); } }
        public By NameFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-name')]"); } }
        public By DescriptionFieldBy { get { return By.XPath("//textarea[contains(@id, 'vip-level-description')]"); } }
        public By ColorFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-color')]"); } }
        public By RankFieldBy { get { return By.XPath("//input[contains(@id, 'vip-level-rank')]"); } }
        public By NewGameButtonBy { get { return By.XPath("//button[text()='New Game']"); } }
        public By GameLimitsBy { get { return By.XPath("//span[text()='Game']"); } }
        public By GameSelectBy { get { return By.XPath("//select[contains(@data-bind, 'options: games')]"); } }
        public By NewCurrencyButtonBy { get { return By.XPath("//button[text()='New Currency']"); } }
        public By CurrencySelectBy { get { return By.XPath("//select[contains(@data-bind, 'options: currencies')]"); } }
        public By MinBetFieldBy { get { return By.XPath("//input[contains(@data-bind, 'value: min')]"); } }
        public By MaxBetFieldBy { get { return By.XPath("//input[contains(@data-bind, 'value: max')]"); } }
        public By AddGameLimitButtonBy { get { return By.XPath("//button[contains(@data-bind, 'click: addGameLimit')]"); } }
        public By ProductListBy { get { return By.XPath("//select[contains(@data-bind, 'options: gameProviders')]"); } }
        public By BetLevelListBy { get { return By.XPath("//select[contains(@data-bind, 'options: betLimits')]"); } }
        public By CurrencyListBy { get { return By.XPath("//select[contains(@data-bind, 'options: currencies')]"); } }
        public By ValidationMessageBy { get { return By.XPath("//span[@data-bind='validationMessage: fields.isDefault']"); } }

        public NewVipLevelForm(IWebDriver driver) : base(driver) {}

        public ViewVipLevelForm Submit(VipLevelData data)
        {
            EnterVipLevelDetails(data);

            return Submit();
        }

        public void EnterVipLevelDetails(VipLevelData data)
        {
            SelectLicenseeBrand(LoadingCompleteBy, LicenseeSelectBy, data.Licensee, BrandSelectBy, data.Brand);

            if (!data.DefaultForNewPlayers)
            {
                var defaultForNewPlayerCheckbox = _driver.FindElementWait(DefaultForNewPlayersCheckboxBy);
                defaultForNewPlayerCheckbox.Click();
            }

            var codeField = _driver.FindElementWait(CodeFieldBy);
            codeField.SendKeys(data.Code);

            var nameField = _driver.FindElementWait(NameFieldBy);
            nameField.SendKeys(data.Name);

            var rankField = _driver.FindElementWait(RankFieldBy);
            rankField.SendKeys(data.Rank.ToString());

            var descriptionField = _driver.FindElementWait(DescriptionFieldBy);
            descriptionField.SendKeys(data.Description);

            var colorPicker = _driver.FindElementWait(By.XPath("//span[contains(@class,'btn-colorpicker')]"));
            colorPicker.Click();

            var color = _driver.FindElementWait(By.XPath(string.Format("//a[@data-color='{0}']", data.DisplayColor)));
            color.Click();
        }

        public void AddProductLimit(string product, string betLevel, string currency)
        {
            var addGameLimitButton = _driver.FindElementWait(AddGameLimitButtonBy);
            addGameLimitButton.Click();

            var productsList = _driver.FindElementWait(ProductListBy);
            var productField = new SelectElement(productsList);
            productField.SelectByText(product);

            var betLevelsList = _driver.FindElementWait(BetLevelListBy);
            var betLevelField = new SelectElement(betLevelsList);
            betLevelField.SelectByText(betLevel);

            var currenciesList = _driver.FindElementWait(CurrencyListBy);
            var currencyField = new SelectElement(currenciesList);
            currencyField.SelectByText(currency);
        }       

        public ViewVipLevelForm Submit()
        {
            _driver.FindElementWait(By.XPath("//button[text()='Save']")).Click();
            return new ViewVipLevelForm(_driver);
        }

        public string ValidationMessage
        {
            get { return _driver.FindElementValue(ValidationMessageBy); }
        }
    }    
}
