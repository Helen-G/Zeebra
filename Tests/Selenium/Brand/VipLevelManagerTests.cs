using System;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class VipLevelManagerTests : SeleniumBaseForAdminWebsite
    {
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private DashboardPage _dashboardPage;
        VipLevelManagerPage _vipLevelsPage;
        private Guid _brandId;
        private Licensee _defaultLicensee;
        private Brand _brand;
        private BrandTestHelper _brandTestHelper;
        private const string CurrencyCode = "CAD";

        public override void BeforeAll()
        {
            base.BeforeAll();
            _brandTestHelper = _container.Resolve<BrandTestHelper>();
            _defaultLicensee = _brandTestHelper.GetDefaultLicensee();
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            //create a brand for a default licensee
            _brandId = _brandTestHelper.CreateBrand(_defaultLicensee, PlayerActivationMethod.Automatic);

            _brandTestHelper.AssignCurrency(_brandId, "CAD");
            var brandQueries = _container.Resolve<BrandQueries>();
            _brand = brandQueries.GetBrandOrNull(_brandId);

            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
        }

        [Test]
        public void Can_create_vip_level_with_product_limit()
        {
            //create a product
            var productName = "product" + TestDataGenerator.GetRandomString(5);
            var productManagerPage = _dashboardPage.Menu.ClickProductManagerMenuItem();
            var newProductForm = productManagerPage.OpenNewProductForm();
            var submittedProductForm = newProductForm.Submit(productName, "Casino", "Token");
            
            //add a product to a licensee
            var licenseeManagerPage = submittedProductForm.Menu.ClickLicenseeManagerItem();
            var editLicenseeForm = licenseeManagerPage.OpenEditLicenseeForm(_defaultLicensee.Name);
            var viewLicenseeForm = editLicenseeForm.EditAssignedProducts(productName);
            viewLicenseeForm.CloseTab("View Licensee");
            
            //assign a product to the default brand
            var supportedProductsPage = licenseeManagerPage.Menu.ClickSupportedProductsMenuItem();
            var manageProductsPage = supportedProductsPage.OpenManageProductsPage();
            var editedLicenseeForm = manageProductsPage.AssignProducts(_defaultLicensee.Name, _brand.Name, new[] { productName });
            
            //add a bet limit to the product
            var betLimitName = TestDataGenerator.GetRandomString(5);
            var betLimitCode = TestDataGenerator.GetRandomString(4);
            var betLimitNameCode = string.Format(betLimitCode + " " + "-" + " " + betLimitName);
            
            var betLevelsPage = editedLicenseeForm.Menu.ClickBetLevelsMenuItem();
            var newBetLevelForm = betLevelsPage.OpenNewBetLevelForm();

            newBetLevelForm.SelectLicensee(_defaultLicensee.Name);
            newBetLevelForm.SelectBrand(_brand.Name);
            newBetLevelForm.SelectProduct(productName);
            newBetLevelForm.AddBetLevelDetails(betLimitName, betLimitCode);
            var submittedBetLevelForm = newBetLevelForm.Submit();
            
            // create a default vip level
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name);
            
            _vipLevelsPage = submittedBetLevelForm.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = _vipLevelsPage.OpenNewVipLevelForm();
            newVipLevelForm.EnterVipLevelDetails(vipLevelData);
            newVipLevelForm.AddProductLimit(productName, betLimitNameCode, CurrencyCode);
            var submittedVipLevelForm = newVipLevelForm.Submit();

            Assert.AreEqual("VIP Level has been created successfully.", submittedVipLevelForm.ConfirmationMessage);
            Assert.AreEqual(vipLevelData.Licensee, submittedVipLevelForm.Licensee);
            Assert.AreEqual(vipLevelData.Brand, submittedVipLevelForm.Brand);
            Assert.AreEqual(vipLevelData.Code, submittedVipLevelForm.Code);
            Assert.AreEqual(vipLevelData.Name, submittedVipLevelForm.Name);
        }

        [Test]
        public void Cannot_create_more_than_one_default_vip_level_per_brand()
        {
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(_defaultLicensee.Name, _brand.Name);
            
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelForm = _vipLevelsPage.OpenNewVipLevelForm();
            var viewVipLevelForm = newVipLevelForm.Submit(vipLevelData);
            viewVipLevelForm.CloseTab("View VIP Level");

            var newVipLevelForm2 = _vipLevelsPage.OpenNewVipLevelForm();
            newVipLevelForm2.Submit(vipLevelData);

            Assert.AreEqual("Default vip level for this brand already exists.", newVipLevelForm2.ValidationMessage);
        }

        [Test]
        public void Can_deactivate_vip_level()
        {
            var vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, "831", false);

            //create a vip level for brand '831'
            _vipLevelsPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newForm = _vipLevelsPage.OpenNewVipLevelForm();
            var submittedForm = newForm.Submit(vipLevelData);
            submittedForm.CloseTab("View VIP Level");

            //deactivate the vip level
            var deactivateDialog = _vipLevelsPage.OpenDeactivateDialog(vipLevelData.Name);
            deactivateDialog.Deactivate();

            Assert.IsTrue(_vipLevelsPage.CheckDeactivatedVipLevelStatus(vipLevelData.Name));
        }
    }
}
