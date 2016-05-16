using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class WalletManagerTests : SeleniumBaseForAdminWebsite
    {
        private string _brandName = null;
        private const string DefaultLicenseeName = "Flycow";
        
        private DashboardPage _dashboardPage;

        public override void BeforeAll()
        {
            base.BeforeAll();
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var defaultLicensee = brandTestHelper.GetDefaultLicensee();
            var brandQueries = _container.Resolve<BrandQueries>();

            //create a brand for a default licensee
            var brandId = brandTestHelper.CreateBrand(defaultLicensee, PlayerActivationMethod.Automatic);
            _brandName = brandQueries.GetBrand(brandId).Name;
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _dashboardPage.BrandFilter.SelectAll();
        }
        
        [Test]
        public void Can_add_product_wallet_to_main_wallet()
        {
            //assign products to the default brand
            var supportedProductsPage = _dashboardPage.Menu.ClickSupportedProductsMenuItem();
            var manageProductsPage = supportedProductsPage.OpenManageProductsPage();
            var products = new[] { "ACS dev", "Mock Casino" };
            var assignedProductsForm = manageProductsPage.AssignProducts(DefaultLicenseeName, _brandName, products);
            assignedProductsForm.CloseTab("View Assigned Products");

            //create a main wallet
            var walletManager = assignedProductsForm.Menu.ClickWalletManagerMenuItem();
            var newWalletForm = walletManager.OpenNewWalletForm();
            var viewWalletForm = newWalletForm.Submit(DefaultLicenseeName, _brandName, products);
            var listOfassignedProductsToProductWallet = viewWalletForm.GetAssignedProducts();
            
            Assert.AreEqual("The wallet has been successfully created", viewWalletForm.ConfirmationMessage);
            Assert.AreEqual(products[0], listOfassignedProductsToProductWallet[0].Text);
            Assert.AreEqual(products[1], listOfassignedProductsToProductWallet[1].Text);
        }
    
    }
}
