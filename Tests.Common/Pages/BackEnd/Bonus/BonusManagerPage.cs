using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Bonus
{
    public class BonusManagerPage : BackendPageBase
    {
        public BonusManagerPage(IWebDriver driver) : base(driver) {}

        public Grid Grid
        {
            get
            {
                return new Grid(_driver, "bonus-name-search", "bonus-name-search-button");
            }
        }

        public AddEditBonusForm OpenNewBonusForm()
        {
            var newButton = _driver.FindElementWait(By.XPath("//div[@id='bonus-grid']//button/span[text()='New']"));
            newButton.Click();
            return new AddEditBonusForm(_driver);
        }

        public void ActivateBonus(string bonusName)
        {
            SelectBonus(bonusName);
            var activationDialog = OpenActivationDialog();
            activationDialog.Activate();
        }

        public void DeactivateBonus(string bonusName)
        {
            SelectBonus(bonusName);
            var deactivationDialog = OpenDeactivationDialog();
            deactivationDialog.Deactivate();
        }

        public bool CheckIfBonusStatusIsActive(string bonusName)
        {
            Grid.SelectRecord(bonusName);

            var bonusStatusXPath = By.XPath("//td[text()='Active']");
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(d =>
            {
                var foundElement = _driver.FindElements(bonusStatusXPath).FirstOrDefault();
                return foundElement != null;
            });
            return true;
        }

        private void SelectBonus(string bonusName)
        {
            Grid.SelectRecord(bonusName);
        }

        private BonusActivationDialog OpenActivationDialog()
        {
            var activateButton = _driver.FindElementWait(By.XPath("//span[text()='Activate']"));
            activateButton.Click();
            return new BonusActivationDialog(_driver);
        }

        private BonusActivationDialog OpenDeactivationDialog()
        {
            var activateButton = _driver.FindElementWait(By.XPath("//span[text()='Deactivate']"));
            activateButton.Click();
            return new BonusActivationDialog(_driver);
        }

        public AddEditBonusForm OpenEditBonusForm(string bonusName)
        {
            Grid.SelectRecord(bonusName);
            var editButton = _driver.FindElementWait(By.XPath("//span[text()='Edit']"));
            editButton.Click();
            return new AddEditBonusForm(_driver);
        }

        public SubmittedBonusForm OpenViewBonusForm(string bonusName)
        {
            Grid.SelectRecord(bonusName);
            var viewButton = _driver.FindElementWait(By.XPath("//span[text()='View']"));
            viewButton.Click();
            return new SubmittedBonusForm(_driver);
        }
    }
}