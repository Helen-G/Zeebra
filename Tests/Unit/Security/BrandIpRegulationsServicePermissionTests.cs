using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using IpRegulationConstants = AFT.RegoV2.Infrastructure.Constants.IpRegulationConstants;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class BrandIpRegulationsServicePermissionTests : PermissionsTestsBase
    {
        private BrandIpRegulationService _brandService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandService = Container.Resolve<BrandIpRegulationService>();
        }

        [Test]
        public void Cannot_execute_BackendIpRegulationService_without_permissions()
        {
            /* Arrange */
            LogWithNewUser(Modules.PlayerManager, Permissions.View);

            /* Act */
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.GetIpRegulations());
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.CreateIpRegulation(new AddBrandIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.UpdateIpRegulation(new EditBrandIpRegulationData()));
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.DeleteIpRegulation(new Guid()));
        }

        [Test]
        public void Cannot_create_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var data = new AddBrandIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            LogWithNewUser(Modules.BrandIpRegulationManager, Permissions.Add);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.CreateIpRegulation(data));
        }

        [Test]
        public void Cannot_update_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var addBrandIpRegulationData = new AddBrandIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            _brandService.CreateIpRegulation(addBrandIpRegulationData);

            var editBrandIpRegulationData = Mapper.DynamicMap<EditBrandIpRegulationData>(addBrandIpRegulationData);

            LogWithNewUser(Modules.BrandIpRegulationManager, Permissions.Edit);

            /*** Act ***/
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.UpdateIpRegulation(editBrandIpRegulationData));
        }

        [Test]
        public void Cannot_delete_brand_ip_regulation_with_invalid_brand()
        {
            /*** Arrange ***/
            var brandTestHelper = Container.Resolve<BrandTestHelper>();

            var licensee = brandTestHelper.CreateLicensee();
            var brand = brandTestHelper.CreateBrand(licensee, isActive: true);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var data = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = brand.Id,
                LicenseeId = licensee.Id,
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = "google.com"
            };

            _brandService.CreateIpRegulation(data);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            LogWithNewUser(Modules.BrandIpRegulationManager, Permissions.Delete);

            /*** Act ***/
            Assert.NotNull(regulation);
            Assert.Throws<InsufficientPermissionsException>(() => _brandService.DeleteIpRegulation(regulation.Id));
        }
    }
}
