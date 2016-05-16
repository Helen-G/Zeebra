using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AFT.RegoV2.AdminApi.Controllers.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Brand.Validators.ContentTranslations;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.AdminApi.Tests.Integration.Steps
{
    [Binding]
    public class BrandSteps : BaseSteps
    {
        private BrandTestHelper BrandHelper { get; set; }
        private SecurityTestHelper SecurityTestHelper { get; set; }

        public BrandSteps()
        {
            BrandHelper = Container.Resolve<BrandTestHelper>();
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            SecurityTestHelper.SignInUser();
        }

        [Given(@"I am logged in and have access token")]
        public void GivenIAmLoggedInAndHaveAccessToken()
        {
            LogInAdminApi("SuperAdmin", "SuperAdmin");
            Token.Should().NotBeNullOrWhiteSpace();
        }

        [When(@"New (.*) brand is created")]
        public void WhenNewBrandIsCreated(string activationStatus)
        {
            var isActive = activationStatus.Equals("activated");
            var licensee = BrandHelper.CreateLicensee();
            ScenarioContext.Current.Add("licenseeId", licensee.Id);
            ScenarioContext.Current.Add("brandId", BrandHelper.CreateBrand(licensee, isActive: isActive).Id);
        }

        [When(@"New country is created")]
        public void WhenNewCountryIsCreated()
        {
            ScenarioContext.Current.Add("countryCode", BrandHelper.CreateCountry(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New culture is created")]
        public void WhenNewCultureIsCreated()
        {
            ScenarioContext.Current.Add("cultureCode", BrandHelper.CreateCulture(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [When(@"New brand currency is created")]
        public void WhenNewBrandCurrencyIsCreated()
        {
            ScenarioContext.Current.Add("currencyCode", BrandHelper.CreateCurrency(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        [Then(@"Available brands are visible to me")]
        public void ThenAvailableBrandsAreVisibleToMe()
        {
            var result = AdminApiProxy.GetUserBrands();

            result.Should().NotBeNull();
            result.Brands.Should().NotBeNull();
            result.Brands.Should().NotBeEmpty();
        }

        [Then(@"Required data to add new brand is visible to me")]
        public void ThenRequiredDataToAddNewBrandIsVisibleToMe()
        {
            var result = AdminApiProxy.GetBrandAddData();

            result.Should().NotBeNull();
        }

        [Then(@"Required data to edit that brand is visible to me")]
        public void ThenRequiredDataToEditThatBrandIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandEditData(brandId);

            result.Should().NotBeNull();
        }

        [Then(@"Required brand data is visible to me")]
        public void ThenRequiredBrandDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandViewData(brandId);

            result.Should().NotBeNull();
        }

        [Then(@"New brand is successfully added")]
        public void ThenNewBrandIsSuccessfullyAdded()
        {
            var licensee = BrandHelper.CreateLicensee();

            var data = new AddBrandData()
            {
                Code = TestDataGenerator.GetRandomString(),
                InternalAccounts = 1,
                EnablePlayerPrefix = true,
                PlayerPrefix = TestDataGenerator.GetRandomString(3),
                Licensee = licensee.Id,
                Name = TestDataGenerator.GetRandomString(),
                PlayerActivationMethod = PlayerActivationMethod.Automatic,
                TimeZoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Type = BrandType.Integrated
            };

            var result = AdminApiProxy.AddBrand(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand data is successfully edited")]
        public void ThenBrandDataIsSuccessfullyEdited()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var newBrandName = TestDataGenerator.GetRandomString(20);
            var newBrandCode = TestDataGenerator.GetRandomString(20);
            var newTimeZoneId = TestDataGenerator.GetRandomTimeZone().Id;
            var newInternalAccountCount = TestDataGenerator.GetRandomNumber(10, 0);
            const string remarks = "Test updating brand";

            var data = new EditBrandData
            {
                Brand = brandId,
                Code = newBrandCode,
                EnablePlayerPrefix = true,
                InternalAccounts = newInternalAccountCount,
                Licensee = licenseeId,
                Name = newBrandName,
                PlayerPrefix = "AAA",
                TimeZoneId = newTimeZoneId,
                Type = BrandType.Deposit,
                Remarks = remarks
            };

            var result = AdminApiProxy.EditBrand(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand countries are visible to me")]
        public void ThenBrandCountriesAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCountries(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand is successfully activated")]
        public void ThenBrandIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new ActivateBrandData
            {
                BrandId = brandId,
                Remarks = "Some remark"
            };

            var result = AdminApiProxy.ActivateBrand(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand is successfully deactivated")]
        public void ThenBrandIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var data = new DeactivateBrandData
            {
                BrandId = brandId,
                Remarks = "Some remark"
            };

            var result = AdminApiProxy.DeactivateBrand(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brands are visible to me")]
        public void ThenBrandsAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("licenseeId");
            var licenseeId = ScenarioContext.Current.Get<Guid>("licenseeId");

            var result = AdminApiProxy.GetBrands(false, new[] {licenseeId});
            result.Should().NotBeNull();
        }

        [Then(@"Brand country assign data is visible to me")]
        public void ThenBrandCountryAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCountryAssignData(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand country is successfully added")]
        public void ThenBrandCountryIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("countryCode");
            var code = ScenarioContext.Current.Get<string>("countryCode");

            var data = new AssignBrandCountryData()
            {
                Brand = brandId,
                Countries = new[] { code }
            };

            var result = AdminApiProxy.AssignBrandCountry(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        //[Then(@"Brands countries list is visible to me")]
        //public void ThenBrandsCountriesListIsVisibleToMe()
        //{
        //    var searchPackage = new SearchPackage
        //    {
        //        PageIndex = 0,
        //        RowCount = 10,
        //        SortASC = true,
        //        SortColumn = "BrandName",
        //        SortSord = "",
        //        SingleFilter = null,
        //        AdvancedFilter = null
        //    };

        //    var result = AdminApiProxy.GetBrandCountriesList(searchPackage);

        //    result.Should().NotBeNull();
        //}

        [Then(@"Brand culture assign data is visible to me")]
        public void ThenBrandCultureAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCultureAssignData(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand culture is successfully added")]
        public void ThenBrandCultureIsSuccessfullyAdded()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var code = ScenarioContext.Current.Get<string>("cultureCode");

            var data = new AssignBrandCultureData()
            {
                Brand = brandId,
                Cultures = new[] { code },
                DefaultCulture = code
            };

            var result = AdminApiProxy.AssignBrandCulture(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand currencies are visible to me")]
        public void ThenBrandCurrenciesAreVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCurrencies(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand currency assign data is visible to me")]
        public void ThenBrandCurrencyAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandCurrencyAssignData(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand product assign data is visible to me")]
        public void ThenBrandProductAssignDataIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("brandId");
            var brandId = ScenarioContext.Current.Get<Guid>("brandId");

            var result = AdminApiProxy.GetBrandProductAssignData(brandId);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand product is successfully assigned")]
        public void ThenBrandProductIsSuccessfullyAssigned()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brand = BrandHelper.CreateBrand(licensee, isActive: true);

            var data = new AssignBrandProductModel
            {
                Brand = brand.Id,
                Products = brand.Products.Select(b => b.BrandId.ToString()).ToArray()
            };

            var result = AdminApiProxy.AssignBrandProduct(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Brand product bet levels are visible to me")]
        public void ThenBrandProductBetLevelsAreVisibleToMe()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brand = BrandHelper.CreateBrand(licensee, isActive: true);

            var result = AdminApiProxy.GetBrandProductBetLevels(brand.Id, brand.Products.First().ProductId);

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Then(@"New content translation is successfully created")]
        public void ThenNewContentTranslationIsSuccessfullyCreated()
        {
            var contentName = TestDataGenerator.GetRandomString();
            var contentSource = TestDataGenerator.GetRandomString();

            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var translation = new AddContentTranslationData()
            {
                ContentName = contentName,
                ContentSource = contentSource,
                Language = cultureCode,
                Translation = TestDataGenerator.GetRandomString()
            };

            var data = new AddContentTranslationModel()
            {
                Languages = new List<string>(),
                Translations = new[] {translation},
                ContentName = contentName,
                ContentSource = contentSource
            };

            var result = AdminApiProxy.CreateContentTranslation(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"I can not execute protected brand methods with insufficient permissions")]
        public void ThenICanNotExecuteProtectedBrandMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);

            //TODO: This is not right way checking if Access forbidden exception has been raised. Consider refactoring.
            Assert.Throws<AggregateException>(() => AdminApiProxy.AddBrand(new AddBrandData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.EditBrand(new EditBrandData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.ActivateBrand(new ActivateBrandData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.DeactivateBrand(new DeactivateBrandData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.AssignBrandCountry(new AssignBrandCountryData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.AssignBrandCulture(new AssignBrandCultureData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.AssignBrandCurrency(new AssignBrandCurrencyData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.CreateContentTranslation(new AddContentTranslationModel()));
            //Assert.Throws<AggregateException>(() => AdminApiProxy.UpdateContentTranslation(new EditContentTranslationData()));
            //Assert.Throws<AggregateException>(() => AdminApiProxy.ActivateContentTranslation(new ActivateContentTranslationData()));
            //Assert.Throws<AggregateException>(() => AdminApiProxy.DeactivateContentTranslation(new DeactivateContentTranslationData()));
            //Assert.Throws<AggregateException>(() => AdminApiProxy.DeleteContentTranslation(new DeleteContentTranslationData()));
        }
    }
}
