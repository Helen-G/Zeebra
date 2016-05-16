using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus.BonusTemplateWizard
{
    public class RulesPage : TemplateWizardPageBase
    {
        public RulesPage(IWebDriver driver) : base(driver)
        {
            _driver.FindElementWait(By.XPath(BaseXPath + "//div[@class='template-wizard']/ul/li[3][@class='active']"));
        }

        public WageringPage Next()
        {
            _nextBtn.Click();

            return new WageringPage(_driver);
        }

        public RulesPage SelectRewardType(BonusRewardType rewardType)
        {
            _driver.FindElementClick(BaseXPath + "//button[contains(@data-bind, 'text: rewardTypeString')]");

            var optionText = string.Empty;
            switch (rewardType)
            {
                case BonusRewardType.Amount:
                    optionText = "Fixed amount";
                    break;
                case BonusRewardType.Percentage:
                    optionText = "Percentage";
                    break;
                case BonusRewardType.TieredAmount:
                    optionText = "Tiered fixed amount";
                    break;
                case BonusRewardType.TieredPercentage:
                    optionText = "Tiered percentage";
                    break;
            }

            _driver.FindElementClick(BaseXPath + "//ul[contains(@data-bind, 'foreach: availableRewardTypes')]//a[text()='" + optionText + "']");

            return this;
        }

        public RulesPage EnterReferFriendsConfiguration(decimal minDepositAmount, decimal wageringCondition)
        {
            var minDepositField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: ReferFriendMinDepositAmount')]"));
            minDepositField.SendKeys(minDepositAmount.ToString());

            var wageringField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: ReferFriendWageringCondition')]"));
            wageringField.SendKeys(wageringCondition.ToString());

            return this;
        }

        public RulesPage EnterHighDepositConfiguration(decimal requiredDeposit, decimal bonusAmount)
        {
            var monthlyDepositField = _driver.FindElements(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: From')]")).Last();
            monthlyDepositField.SendKeys(requiredDeposit.ToString());

            var rewardField = _driver.FindElements(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: Reward')]")).Last();
            rewardField.SendKeys(bonusAmount.ToString());

            var notificationThresholdField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: NotificationPercentThreshold')]"));
            notificationThresholdField.SendKeys("90");

            return this;
        }

        public RulesPage AddBonusTier()
        {
            var addTierButton = _driver.FindElementWait(By.XPath(BaseXPath + "//button[contains(@data-bind, 'click: addBonusTier')]"));
            addTierButton.Click();

            return this;
        }

        public RulesPage EnterBonusTier(decimal rewardAmountOrPercent, decimal fromAmount = decimal.Zero, decimal maxTierReward = decimal.Zero)
        {
            if (fromAmount != decimal.Zero)
            {
                var fromField = _driver.FindElements(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vFrom')]")).Last();
                fromField.SendKeys(fromAmount.ToString());
            }

            var rewardField = _driver.FindElements(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vReward, numeric: vReward')]")).Last();
            rewardField.SendKeys(rewardAmountOrPercent.ToString());

            if (maxTierReward != decimal.Zero)
            {
                var maxAmountField =
                    _driver.FindElements(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vMaxAmount')]"))
                        .Last();
                maxAmountField.SendKeys(maxTierReward.ToString());
            }

            return this;
        }

        public RulesPage SelectFundInWallet(string walletName)
        {
            var widget = new MultiSelectWidget(_driver, By.XPath(BaseXPath + "//div[contains(@data-bind, 'items: FundInWallets')]"));
            widget.SelectFromMultiSelect(walletName);

            return this;
        }

        public RulesPage SelectCurrency(string currency)
        {
            var widget = new MultiSelectWidget(_driver, By.XPath(BaseXPath + "//div[contains(@data-bind, 'items: currencies')]"));
            widget.SelectFromMultiSelect(currency);

            return this;
        }

        public RulesPage LimitMaxTotalBonusAmount(decimal limitAmount)
        {
            var bonusTemplateNameField = _driver.FindElementWait(By.XPath(BaseXPath + "//input[contains(@data-bind, 'value: vRewardAmountLimit')]"));
            bonusTemplateNameField.SendKeys(limitAmount.ToString());

            return this;
        }

        public SummaryPage NavigateToSummary()
        {
            return Next().NavigateToSummary();
        }
    }
}