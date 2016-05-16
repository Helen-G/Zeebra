using System;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    class LicenseeTests : AdminWebsiteUnitTestsBase
    {
        private FakeBrandRepository _fakeBrandRepository;
        private LicenseeCommands _licenseeCommands;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _fakeBrandRepository = Container.Resolve<FakeBrandRepository>();
            _licenseeCommands = Container.Resolve<LicenseeCommands>();
            Container.Resolve<SecurityTestHelper>().SignInUser();
        }

        [Test]
        public void Can_activate_licensee()
        {
            var licensee = CreateLicensee(LicenseeStatus.Inactive);

            _licenseeCommands.Activate(licensee.Id, "test");

            Assert.AreEqual(licensee.Status, LicenseeStatus.Active);
        }

        [Test]
        public void Can_deactivate_licensee()
        {
            var licensee = CreateLicensee(LicenseeStatus.Active);

            _licenseeCommands.Deactivate(licensee.Id, "test");

            Assert.AreEqual(licensee.Status, LicenseeStatus.Deactivated);
        }

        [Test]
        public void Cannot_activate_licensee_after_contract_expired()
        {
            var licensee = CreateLicensee(LicenseeStatus.Deactivated);
            licensee.ContractEnd = DateTimeOffset.UtcNow.AddDays(-1);
            _fakeBrandRepository.SaveChanges();


            Action action = () => _licenseeCommands.Activate(licensee.Id, "test");

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("cantActivateContractExpired"));
        }

        [Test]
        public void Cannot_activate_active_licensee()
        {
            var licensee = CreateLicensee(LicenseeStatus.Active);

            Action action = () => _licenseeCommands.Activate(licensee.Id, "test");

            action.ShouldThrow<RegoValidationException>()
                .Where(x => x.Message.Contains("notInactive"));
        }

        [Test]
        public void Cannot_deactivate_deactivated_licensee()
        {
            var licensee = CreateLicensee(LicenseeStatus.Deactivated);

            Action action = () => _licenseeCommands.Deactivate(licensee.Id, "test");

            action.ShouldThrow<RegoValidationException>()
                 .Where(x => x.Message.Contains("notActive"));
        }

        private Licensee CreateLicensee(LicenseeStatus status)
        {
            var randomString = TestDataGenerator.GetRandomString();

            var licensee = new Licensee
            {
                Id = Guid.NewGuid(),
                Name = "Name" + randomString,
                AffiliateSystem = false,
                CompanyName = "CName" + randomString,
                ContractStart = DateTimeOffset.UtcNow.AddDays(-7),
                Email = TestDataGenerator.GetRandomEmail(),
                AllowedBrandCount = 10,
                AllowedWebsiteCount = 10,
                TimezoneId = TestDataGenerator.GetRandomTimeZone().Id,
                Status = status,
            };            

            licensee.Contracts.Add(new Contract
            {
                Id = Guid.NewGuid(),
                LicenseeId = licensee.Id,
                Licensee = licensee,
                StartDate = licensee.ContractStart,
                EndDate = licensee.ContractEnd,
                IsCurrentContract = true
            });

            _fakeBrandRepository.Licensees.Add(licensee);
            _fakeBrandRepository.SaveChanges();

            return licensee;
        }
    }
}
