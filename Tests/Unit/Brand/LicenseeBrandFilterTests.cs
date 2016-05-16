using System;
using System.Linq;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Brand
{
    public class LicenseeBrandFilterTests : AdminWebsiteUnitTestsBase
    {
        private UserService _userService;
        private ISecurityRepository _securityRepository;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _securityRepository = Container.Resolve<ISecurityRepository>();
            _userService = Container.Resolve<UserService>();
            _securityRepository.Users.Add(new User{ Id = Guid.NewGuid() });
            _securityRepository.SaveChanges();            
        }

        [Test]
        public void Can_get_licensee_filter_selections()
        {
            var user = _securityRepository.Users.First();

            var licensees = AddLicenseeSelections();            

            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(user.Id).ToArray();

            Assert.That(licenseeFilterSelections.Count(), Is.EqualTo(licensees.Count()));
            Assert.That(licenseeFilterSelections.All(licensees.Contains), Is.True);
        }

        [Test]
        public void Can_set_licensee_filter_selections()
        {
            var user = _securityRepository.Users.First();

            var licenseeIds = GetGuids();

            _userService.SetLicenseeFilterSelections(user.Id, licenseeIds);

            var licenseeFilterSelections = _securityRepository.Users.First().LicenseeFilterSelections;

            Assert.That(licenseeFilterSelections.Count(), Is.EqualTo(licenseeIds.Count()));
            Assert.That(licenseeFilterSelections.All(x => licenseeIds.Contains(x.LicenseeId)), Is.True);
        }

        [Test]
        public void Can_get_brand_filter_selections()
        {
            var user = _securityRepository.Users.First();

            var brands = AddBrandSelections();

            var brandFilterSelections = _userService.GetBrandFilterSelections(user.Id).ToArray();

            Assert.That(brandFilterSelections.Count(), Is.EqualTo(brands.Count()));
            Assert.That(brandFilterSelections.All(brands.Contains), Is.True);
        }

        [Test]
        public void Can_set_brand_filter_selections()
        {
            var user = _securityRepository.Users.First();

            var brandIds = GetGuids();

            _userService.SetBrandFilterSelections(user.Id, brandIds);

            var brandFilterSelections = _securityRepository.Users.First().BrandFilterSelections;

            Assert.That(brandFilterSelections.Count(), Is.EqualTo(brandIds.Count()));
            Assert.That(brandFilterSelections.All(x => brandIds.Contains(x.BrandId)), Is.True);
        }

        private Guid[] GetGuids(int number = 5)
        {
            var guids = new Guid[number];

            for (var i = 0; i < number; i++)
            {
                guids[i] = Guid.NewGuid();
            }

            return guids;
        }

        private Guid[] AddLicenseeSelections(int number = 5)
        {
            var guids = GetGuids(number);
            var user = _securityRepository.Users.First();

            for (var i = 0; i < number; i++)
            {
                user.LicenseeFilterSelections.Add(new LicenseeFilterSelection
                {
                    User = user,
                    UserId = user.Id,
                    LicenseeId = guids[i]
                });
            }

            _securityRepository.SaveChanges();

            return guids;
        }

        private Guid[] AddBrandSelections(int number = 5)
        {
            var guids = GetGuids(number);
            var user = _securityRepository.Users.First();

            for (var i = 0; i < number; i++)
            {
                user.BrandFilterSelections.Add(new BrandFilterSelection
                {
                    User = user,
                    UserId = user.Id,
                    BrandId = guids[i]
                });
            }

            _securityRepository.SaveChanges();

            return guids;
        }
    }
}
