using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices.Data;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Brand.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class BrandTests : BrandTestsBase
    {
        private Country Country { get; set; }
        private Culture Culture { get; set; }
        private Currency Currency { get; set; }
        
        public override void BeforeEach()
        {
            base.BeforeEach();

            Country = BrandHelper.CreateCountry("CA", "Canada");
            Culture = BrandHelper.CreateCulture("en-CA", "English (Canada)");
            Currency = BrandHelper.CreateCurrency("CAD", "Canadian Dollar");
        }
       
        [Test]
        public void Can_Activate_Brand()
        {
            var brand = BrandHelper.CreateBrand();
            BrandCommands.ActivateBrand(brand.Id, "remarks");
            brand = BrandQueries.GetBrandOrNull(brand.Id);            
            
            Assert.That(brand.Status, Is.EqualTo(BrandStatus.Active));
        }
        
        [Test]
        public void Cannot_Activate_Brand_Without_VIP_Level()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brandId = BrandHelper.CreateBrand(licensee, PlayerActivationMethod.Automatic);

            BrandHelper.AssignCountry(brandId, Country.Code);
            BrandHelper.AssignCurrency(brandId, Currency.Code);
            BrandHelper.AssignCulture(brandId, Culture.Code);
            PaymentHelper.CreateBank(brandId, Country.Code);
            PaymentHelper.CreateBankAccount(brandId, Currency.Code);
            PaymentHelper.CreatePaymentLevel(brandId, Currency.Code);
            BrandHelper.CreateWallet(licensee.Id, brandId);

            Action action = () => BrandCommands.ActivateBrand(brandId, "remarks");

            action.ShouldThrow<ValidationException>()
                .Where(x => x.Message.Contains("noDefaultVipLevel"));
        }

        [Test]
        public void Cannot_Activate_Brand_Without_Payment_Level()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brandId = BrandHelper.CreateBrand(licensee, PlayerActivationMethod.Automatic);

            BrandHelper.AssignCountry(brandId, Country.Code);
            BrandHelper.AssignCurrency(brandId, Currency.Code);
            BrandHelper.AssignCulture(brandId, Culture.Code);
            BrandHelper.CreateWallet(licensee.Id, brandId);
            BrandHelper.CreateVipLevel(brandId);
            BrandHelper.CreateRiskLevel(brandId);
            BrandHelper.AssignProducts(brandId, new [] { licensee.Products.First().ProductId });

            Action action = () => BrandCommands.ActivateBrand(brandId, "remarks");

            action.ShouldThrow<ValidationException>()
                .Where(x => x.Message.Contains("noDefaultPaymentLevels"));
        }

        [Test]
        public void Cannot_Activate_Brand_Without_Currency()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brandId = BrandHelper.CreateBrand(licensee, PlayerActivationMethod.Automatic);

            //BrandHelper.AssignCurrency(brandId, "CAD");
            BrandHelper.AssignCountry(brandId, Country.Code);
            BrandHelper.AssignCulture(brandId, Culture.Code);
            PaymentHelper.CreateBank(brandId, Country.Code);
            PaymentHelper.CreateBankAccount(brandId, Currency.Code);
            
            Action action = () =>
            {
                PaymentHelper.CreatePaymentLevel(brandId, Currency.Code);
                BrandHelper.CreateWallet(licensee.Id, brandId);
                BrandHelper.CreateVipLevel(brandId);
                BrandCommands.ActivateBrand(brandId, "remarks");
            };

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("noAssignedCurrency"));
        }

        [Test]
        public void Cannot_Activate_Brand_Without_Culture()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brandId = BrandHelper.CreateBrand(licensee, PlayerActivationMethod.Automatic);

            BrandHelper.AssignCountry(brandId, Country.Code);
            BrandHelper.AssignCurrency(brandId, Currency.Code);
            PaymentHelper.CreateBank(brandId, Country.Code);
            PaymentHelper.CreateBankAccount(brandId, Currency.Code);
            PaymentHelper.CreatePaymentLevel(brandId, Currency.Code);
            BrandHelper.CreateWallet(licensee.Id, brandId);
            BrandHelper.CreateVipLevel(brandId);

            Action action = () => BrandCommands.ActivateBrand(brandId, "remarks");

            action.ShouldThrow<ValidationException>()
                .Where(x => x.Message.Contains("noAssignedLanguage"));
        }

        [Test]
        public void Cannot_Activate_Brand_Without_Country()
        {
            var licensee = BrandHelper.CreateLicensee();
            var brandId = BrandHelper.CreateBrand(licensee, PlayerActivationMethod.Automatic);

            BrandHelper.AssignCurrency(brandId, Currency.Code);
            BrandHelper.AssignCulture(brandId, Culture.Code);
            PaymentHelper.CreateBank(brandId, Country.Code);
            PaymentHelper.CreateBankAccount(brandId, Currency.Code);
            PaymentHelper.CreatePaymentLevel(brandId, Currency.Code);
            BrandHelper.CreateWallet(licensee.Id, brandId);
            BrandHelper.CreateVipLevel(brandId);

            Action action = () => BrandCommands.ActivateBrand(brandId, "remarks");

            action.ShouldThrow<ValidationException>()
                .Where(x => x.Message.Contains("noAssignedCountry"));
        }

        [Test]
        public void Cannot_activate_brand_over_licensee_brand_limit()
        {
            var licensee = BrandHelper.CreateLicensee();
            licensee.AllowedBrandCount = 1;
            var brand = BrandHelper.CreateBrand(licensee);
            BrandCommands.ActivateBrand(brand.Id, "remarks");

            var newBrand = BrandHelper.CreateBrand(BrandRepository.Licensees.Single(x => x.Id == licensee.Id));

            BrandHelper.AssignCountry(newBrand.Id, Country.Code);
            BrandHelper.AssignCulture(newBrand.Id, Culture.Code);
            PaymentHelper.CreatePaymentLevel(newBrand.Id, Currency.Code);
            BrandHelper.CreateWallet(licensee.Id, newBrand.Id);

            Action action = () => BrandCommands.ActivateBrand(newBrand.Id, "remarks");

            action.ShouldThrow<ValidationException>()
                .Where(x => x.Message.Contains("licenseeBrandLimitExceeded"));
        }

        [Test]
        public void Can_assign_brand_culture()
        {
            var brand = BrandHelper.CreateBrand();

            BrandCommands.AssignBrandCulture(new AssignBrandCultureData
            {
                Brand = brand.Id,
                Cultures = new []{Culture.Code},
                DefaultCulture = Culture.Code
            });

            brand = BrandQueries.GetBrand(brand.Id);
            
            Assert.That(brand.BrandCultures.Count, Is.EqualTo(1));
            Assert.That(brand.DefaultCulture, Is.EqualTo(Culture.Code));

            var assignedCulture = brand.BrandCultures.First().Culture;

            Assert.That(assignedCulture.Code, Is.EqualTo(Culture.Code));
            Assert.That(assignedCulture.Name, Is.EqualTo(Culture.Name));
        }

        [Test]
        public void Can_assign_brand_country()
        {
            var brand = BrandTestHelper.CreateBrand();

            BrandCommands.AssignBrandCountry(new AssignBrandCountryData
            {
                Brand = brand.Id,
                Countries = new []{ Country.Code}
            });

            brand = BrandQueries.GetBrand(brand.Id);

            Assert.That(brand.BrandCountries.Count, Is.EqualTo(1));

            var assignedCountry = brand.BrandCountries.First().Country;

            Assert.That(assignedCountry.Code, Is.EqualTo(Country.Code));
            Assert.That(assignedCountry.Name, Is.EqualTo(Country.Name));
        }

        [Test]
        public void Can_assign_brand_currency()
        {
            var brand = BrandTestHelper.CreateBrand();

            BrandCommands.AssignBrandCurrency(new AssignBrandCurrencyData
            {
                Brand = brand.Id,
                Currencies = new[] { Currency.Code }
            });

            brand = BrandQueries.GetBrand(brand.Id);

            Assert.That(brand.BrandCurrencies.Count, Is.EqualTo(1));

            var assignedCurrency = brand.BrandCurrencies.First().Currency;

            Assert.That(assignedCurrency.Code, Is.EqualTo(Currency.Code));
            Assert.That(assignedCurrency.Name, Is.EqualTo(Currency.Name));
        }

        [Test]
        public void Can_assign_brand_product()
        {
            var productId = GamesTestHelper.CreateGameProvider().Id;
            var brand = BrandTestHelper.CreateBrand();
            
            brand.Licensee.Products.Add(new LicenseeProduct
            {
                ProductId = productId
            });

            BrandRepository.SaveChanges();

            BrandCommands.AssignBrandProducts(new AssignBrandProductsData
            {
                BrandId = brand.Id,
                ProductsIds = new[] {productId }
            });

            brand = BrandQueries.GetBrand(brand.Id);

            var brandProduct = brand.Products.SingleOrDefault(x => x.ProductId == productId);

            Assert.That(brandProduct, Is.Not.Null);
        }
    }
}
