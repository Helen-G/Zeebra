using System;
using System.Net;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data.Admin;
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
    public class AdminSteps : BaseSteps
    {
        private PaymentTestHelper PaymentHelper { get; set; }

        public AdminSteps()
        {
            PaymentHelper = Container.Resolve<PaymentTestHelper>();
        }

        //[Then(@"Available countries are visible to me")]
        //public void ThenAvailableCountriesAreVisibleToMe()
        //{
        //    var searchPackage = new SearchPackage
        //    {
        //        PageIndex = 0,
        //        RowCount = 10,
        //        SortASC = true,
        //        SortColumn = "",
        //        SortSord = "",
        //        SingleFilter = null,
        //        AdvancedFilter = null
        //    };

        //    var result = AdminApiProxy.GetCountriesList(searchPackage);

        //    result.Should().NotBeNull();
        //}

        [Then(@"Country by code is visible to me")]
        public void ThenCountryByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("countryCode");
            var countryCode = ScenarioContext.Current.Get<string>("countryCode");

            var result = AdminApiProxy.GetCountryByCode(countryCode);

            result.Should().NotBeNull();
        }

        [Then(@"Country data is successfully saved")]
        public void ThenCountryDataIsSuccessfullySaved()
        {
            var data = new EditCountryData
            {
                Code = TestDataGenerator.GetRandomString(3),
                Name = TestDataGenerator.GetRandomString(),
            };

            var result = AdminApiProxy.SaveCountry(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Country is successfully deleted")]
        public void ThenCountryIsSuccessfullyDeleted()
        {
            ScenarioContext.Current.Should().ContainKey("countryCode");
            var countryCode = ScenarioContext.Current.Get<string>("countryCode");

            var data = new DeleteCountryData() {Code = countryCode};

            var result = AdminApiProxy.DeleteCountry(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        //[Then(@"Available cultures are visible to me")]
        //public void ThenAvailableCulturesAreVisibleToMe()
        //{
        //    ScenarioContext.Current.Pending();
        //}

        [Then(@"Culture by code is visible to me")]
        public void ThenCultureByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var result = AdminApiProxy.GetCultureByCode(cultureCode);

            result.Should().NotBeNull();
        }

        [Then(@"Culture is successfully activated")]
        public void ThenCultureIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var deactivateCulturedata = new DeactivateCultureData()
            {
                Code = cultureCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            AdminApiProxy.DeactivateCulture(deactivateCulturedata);

            var activateCulturedata = new ActivateCultureData()
            {
                Code = cultureCode, 
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ActivateCulture(activateCulturedata);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Culture is successfully deactivated")]
        public void ThenCultureIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("cultureCode");
            var cultureCode = ScenarioContext.Current.Get<string>("cultureCode");

            var data = new DeactivateCultureData()
            {
                Code = cultureCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.DeactivateCulture(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Culture data is successfully saved")]
        public void ThenCultureDataIsSuccessfullySaved()
        {
            var data = new EditCultureData
            {
                Code = TestDataGenerator.GetRandomString(3),
                Name = TestDataGenerator.GetRandomString(),
                NativeName = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.SaveCulture(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [When(@"New currency is created")]
        public void WhenNewCurrencyIsCreated()
        {
            ScenarioContext.Current.Add("currencyCode", PaymentHelper.CreateCurrency(TestDataGenerator.GetRandomString(3), TestDataGenerator.GetRandomString(5)).Code);
        }

        //[Then(@"Available currencies are visible to me")]
        //public void ThenAvailableCurrenciesAreVisibleToMe()
        //{
        //    ScenarioContext.Current.Pending();
        //}

        [Then(@"Currency by code is visible to me")]
        public void ThenCurrencyByCodeIsVisibleToMe()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var result = AdminApiProxy.GetCurrencyByCode(currencyCode);

            result.Should().NotBeNull();
        }

        [Then(@"Currency is successfully activated")]
        public void ThenCurrencyIsSuccessfullyActivated()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var data = new ActivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.ActivateCurrency(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Currency is successfully deactivated")]
        public void ThenCurrencyIsSuccessfullyDeactivated()
        {
            ScenarioContext.Current.Should().ContainKey("currencyCode");
            var currencyCode = ScenarioContext.Current.Get<string>("currencyCode");

            var activateCurrencyData = new ActivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            AdminApiProxy.ActivateCurrency(activateCurrencyData);

            var deactivateCurrencyData = new DeactivateCurrencyData()
            {
                Code = currencyCode,
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.DeactivateCurrency(deactivateCurrencyData);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"Currency data is successfully saved")]
        public void ThenCurrencyDataIsSuccessfullySaved()
        {
            var data = new EditCurrencyData
            {
                Code = TestDataGenerator.GetRandomString(3),
                Name = TestDataGenerator.GetRandomString(),
                Remarks = TestDataGenerator.GetRandomString()
            };

            var result = AdminApiProxy.SaveCurrency(data);

            result.Should().NotBeNull();
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
        }

        [Then(@"I can not execute protected admin methods with insufficient permissions")]
        public void ThenICanNotExecuteProtectedAdminMethodsWithInsufficientPermissions()
        {
            LogWithNewUser(Modules.VipLevelManager, Permissions.View);

            //TODO: This is not right way checking if Access forbidden exception has been raised. Consider refactoring.
            Assert.Throws<AggregateException>(() => AdminApiProxy.SaveCountry(new EditCountryData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.DeleteCountry(new DeleteCountryData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.ActivateCulture(new ActivateCultureData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.DeactivateCulture(new DeactivateCultureData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.SaveCulture(new EditCultureData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.ActivateCurrency(new ActivateCurrencyData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.DeactivateCulture(new DeactivateCultureData()));
            Assert.Throws<AggregateException>(() => AdminApiProxy.SaveCurrency(new EditCurrencyData()));
        }
    }
}