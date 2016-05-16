using System;
using System.Linq;
using System.Threading;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Infrastructure.DataAccess.Player.Repository;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class BetLimitTests : SeleniumBaseForAdminWebsite
    {
        private BrandCommands _brandCommands;
        private BrandQueries _brandQueries;

        const string licensee = "Flycow";
        const string brand = "138";
        private static readonly Guid brandId = Guid.Parse("00000000-0000-0000-0000-000000000138");
        const string gameProvider = "Mock Casino";
        const string currency = "CAD";

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandCommands = _container.Resolve<BrandCommands>();
            _brandQueries = _container.Resolve<BrandQueries>();
        }

        [Test]
        public void Can_get_bet_limit_for_player()
        {
            var betLimit = TestDataGenerator.GetRandomString();
            var vipData = TestDataGenerator.CreateValidVipLevelData(licensee, brand, false);

            //create bet limit
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var betLimitManager = dashboardPage.Menu.ClickBetLevelsMenuItem();
            var newBetLimitForm = betLimitManager.OpenNewBetLevelForm();
            newBetLimitForm.SelectLicensee(licensee);
            newBetLimitForm.SelectBrand(brand);
            newBetLimitForm.SelectProduct(gameProvider);
            newBetLimitForm.AddBetLevelDetails(betLimit, betLimit);
            var viewBetLimitForm = newBetLimitForm.Submit();

            // create vip level
            var vipLevelManager = viewBetLimitForm.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = vipLevelManager.OpenNewVipLevelForm();
            newVipLevelForm.EnterVipLevelDetails(vipData);
            var betLevelNameCode = string.Format("{0} - {0}", betLimit);
            newVipLevelForm.AddProductLimit(gameProvider, betLevelNameCode, currency);
            newVipLevelForm.Submit();

            Thread.Sleep(5000);

            // Make new vip level default
            var brandEntity = _brandQueries.GetBrand(brandId);
            var defaultVipLevelId = brandEntity.DefaultVipLevelId;
            var newVipLevel = _brandQueries.GetVipLevels().Single(o => o.Code == vipData.Code);
            _brandCommands.DeactivateVipLevel(defaultVipLevelId.Value, "-", newVipLevel.Id);

            Thread.Sleep(5000);

            //create player
            var brandLoginPage = new MemberWebsiteLoginPage(_driver);
            brandLoginPage.NavigateToMemberWebsite();
            var registerPage = brandLoginPage.GoToRegisterPage();
            var playerData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite(currency);
            registerPage.Register(playerData);

            // log in as the player to the member website and choose a game
            brandLoginPage = new MemberWebsiteLoginPage(_driver);
            brandLoginPage.NavigateToMemberWebsite();
            var playerProfilePage = brandLoginPage.Login(playerData.Username, playerData.Password);
            var gameListPage = playerProfilePage.Menu.ClickPlayGamesMenu();
            var gamePage = gameListPage.StartGame("Roulette");

            Assert.AreEqual(betLimit, gamePage.BetLimitCode);
        }
    }
}
