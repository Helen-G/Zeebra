using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd
{
    public class ChangeVipLevelDialog : BackendPageBase
    {
        public ChangeVipLevelDialog(IWebDriver driver) : base(driver) { }

        public static By VipLevelSelect = By.XPath("//select[contains(@data-bind, 'options: vipLevels')]");
        public static By Remarks = By.XPath("//textarea[@data-bind='value:remarks']");

        public static By Save =
            By.XPath(
                "//div[contains(@data-view, 'player-manager/change-vip-level-dialog')]//button[contains(@data-bind, 'click: ok')]");

        public static By Close = By.XPath("//div[contains(@data-view, 'player-manager/change-vip-level-dialog')]//button[contains(@data-bind, 'click: cancel')]");

        public static By ConfirmYes = By.XPath("//div[contains(@data-view, 'plugins/messageBox')]//button[text()='Yes']");



        public void Submit(string newVipLevel, string remarks = null)
        {
            var selectList = _driver.FindElementWait(VipLevelSelect);
            var selectElement = new SelectElement(selectList);
            selectElement.SelectByText(newVipLevel);

            if (remarks != null)
            {
                var remarksField = _driver.FindElementWait(Remarks);
                remarksField.Clear();
                remarksField.SendKeys(remarks);
            }

            var saveButton = _driver.FindElementWait(Save);
            saveButton.Click();

            var confirmYesButton = _driver.FindElementWait(ConfirmYes);
            confirmYesButton.Click();

            var closeButton = _driver.FindElementWait(Close);
            closeButton.Click();
        }
    }
}