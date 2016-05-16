using System;
using System.Linq;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    internal class CountryTests : BrandTestsBase
    {
        [Test]
        public void Can_create_country()
        {
            var code = TestDataGenerator.GetRandomString(2);
            var name = TestDataGenerator.GetRandomString();

            BrandCommands.CreateCountry(code, name);

            var country = BrandRepository.Countries.SingleOrDefault(x => x.Code == code);

            Assert.That(country, Is.Not.Null);
            Assert.That(country.Code, Is.EqualTo(code));
            Assert.That(country.Name, Is.EqualTo(name));
        }

        [Test]
        public void Can_update_country()
        {
            var code = TestDataGenerator.GetRandomString(2);
            var name = TestDataGenerator.GetRandomString();

            BrandCommands.CreateCountry(code, name);

            var newName = TestDataGenerator.GetRandomString();

            BrandCommands.UpdateCountry(code, newName);

            var country = BrandRepository.Countries.Single(x => x.Code == code);

            Assert.That(country.Name, Is.EqualTo(newName));
        }
        
        [Test]
        public void Cannot_update_unknown_country()
        {
            Action action = () => BrandCommands.UpdateCountry(
                TestDataGenerator.GetRandomString(),
                TestDataGenerator.GetRandomString());

            action.ShouldThrow<RegoException>()
                .WithMessage("Country not found");
        }

        [Test]
        public void Can_delete_country()
        {
            var code = TestDataGenerator.GetRandomString(2);
            var name = TestDataGenerator.GetRandomString();

            BrandCommands.CreateCountry(code, name);

            BrandCommands.DeleteCountry(code);

            var country = BrandRepository.Countries.SingleOrDefault(x => x.Code == code);

            Assert.That(country, Is.Null);
        }
    }
}