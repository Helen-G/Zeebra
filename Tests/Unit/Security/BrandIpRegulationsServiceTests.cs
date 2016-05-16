using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using IpRegulationConstants = AFT.RegoV2.Infrastructure.Constants.IpRegulationConstants;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class BrandIpRegulationsServiceTests : SecurityTestsBase
    {
        private BrandIpRegulationService _brandService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _brandService = Container.Resolve<BrandIpRegulationService>();
            
        }

        [Test]
        public void Can_create_brand_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            /*** Act ***/
            _brandService.CreateIpRegulation(brandIpRegulation);

            /*** Assert ***/
            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            Assert.IsNotNull(regulation);
        }

        [Test]
        public void Cannot_create_brand_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to create new IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Add, Modules.BrandIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions: sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to create ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BrandIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            /*** Act ***/
            // Signin user that has sufficient permissions to create new brand IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method CreateIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _brandService.CreateIpRegulation(brandIpRegulation));


            // Signin user that has insufficient permissions to create new brand IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method CreateIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _brandService.CreateIpRegulation(brandIpRegulation));
        }

        [Test]
        public void Can_update_brand_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            var editIpAddress = TestDataGenerator.GetRandomIpAddress();
            var editDescription = TestDataGenerator.GetRandomString(20);

            var editData = new EditBrandIpRegulationData
            {
                Id = regulation.Id,
                BrandId = Brand.Id,
                IpAddress = editIpAddress,
                Description = editDescription,
                BlockingType = IpRegulationConstants.BlockingTypes.LoginRegistration
            };

            /*** Act ***/
            _brandService.UpdateIpRegulation(editData);

            /*** Assert ***/
            var updatedRegulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == editIpAddress);

            Assert.IsNotNull(updatedRegulation);
            Assert.AreEqual(updatedRegulation.IpAddress, editIpAddress);
            Assert.AreEqual(updatedRegulation.Description, editDescription);
            Assert.AreEqual(updatedRegulation.BlockingType, IpRegulationConstants.BlockingTypes.LoginRegistration);
        }

        [Test]
        public void Cannot_update_brand_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to update IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Edit, Modules.BrandIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions: sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to update ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BrandIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            var editIpAddress = TestDataGenerator.GetRandomIpAddress();
            var editDescription = TestDataGenerator.GetRandomString(20);

            var editData = new EditBrandIpRegulationData
            {
                Id = regulation.Id,
                BrandId = Brand.Id,
                IpAddress = editIpAddress,
                Description = editDescription,
                BlockingType = IpRegulationConstants.BlockingTypes.LoginRegistration
            };

            /*** Act ***/
            // Signin user that has sufficient permissions to update brand IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method UpdateIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _brandService.UpdateIpRegulation(editData));


            // Signin user that has insufficient permissions to update brand IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method UpdateIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _brandService.UpdateIpRegulation(editData));
        }

        [Test]
        public void Can_delete_brand_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            /*** Act ***/
            _brandService.DeleteIpRegulation(regulation.Id);

            /*** Assert ***/
            var deletedRegulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            Assert.IsNull(deletedRegulation);
        }

        [Test]
        public void Cannot_delete_brand_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to delete IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Delete, Modules.BrandIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions: sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to delete ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BrandIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            const string redirectionUrl = "google.com";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddress,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            var regulation = _brandService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            /*** Act ***/
            // Signin user that has sufficient permissions to delete backend IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method DeleteIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _brandService.DeleteIpRegulation(regulation.Id));

            // Signin user that has insufficient permissions to delete backend IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method DeleteIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _brandService.DeleteIpRegulation(regulation.Id));
        }


        [Test]
        public void Can_verify_unique_ip_address_for_brand_website()
        {
            // *** Arrange ***
            const string ipAddressRange = "192.168.5.10-20";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressRange,
                BrandId = Brand.Id
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // *** Act ***
            var isIpAddressWithinRegulationRange = _brandService.IsIpAddressUnique("192.168.5.15");
            var isIpAddressOutOfRegulationRange = _brandService.IsIpAddressUnique("192.168.5.52");
            var isIpAddressRangeIntersectRegulationRange = _brandService.IsIpAddressUnique("192.168.5.15-25");
            var isIpAddressRangeDontIntersectRegulationRange = _brandService.IsIpAddressUnique("192.168.5.25-30");

            // *** Assert ***
            Assert.False(isIpAddressWithinRegulationRange);
            Assert.True(isIpAddressOutOfRegulationRange);
            Assert.False(isIpAddressRangeIntersectRegulationRange);
            Assert.True(isIpAddressRangeDontIntersectRegulationRange);
        }

        [Test]
        public void Can_verify_ip_address_range_lower_and_upper_bounds_for_brand_website()
        {
            // *** Arrange ***
            const string ipAddressRange = "192.168.5.10-20";

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressRange,
                BrandId = Brand.Id
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // *** Act ***
            var isLowerBoundBlocked = !_brandService.VerifyIpAddress("192.168.5.10").Allowed;
            var isUpperBoundBlocked = !_brandService.VerifyIpAddress("192.168.5.20").Allowed;

            // *** Assert ***
            Assert.True(isLowerBoundBlocked);
            Assert.True(isUpperBoundBlocked);
        }

        public static class IpAddressRangeSeparators
        {
            public static readonly string Slash = "/";
            public static readonly string Dash = "-";
        }

        [Test]
        public void Block_brand_website_for_ip_address_ranges_specified_with_slashes()
        {
            // *** Arrange ***
            var ipAddressRangeWithSlash = TestDataGenerator.GetRandomIpAddressV4Range(IpAddressRangeSeparators.Slash);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressRangeWithSlash,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // Expand ip address range into list of ip addresses, e.g. 192.168.5.10/12 is expanded into [192.168.5.10, 192.168.5.11]
            var expandedIpAddressRange = IpRegulationRangeHelper.ExpandIpAddressRange(ipAddressRangeWithSlash).ToList();

            // *** Act ***
            var ipAddressRangeVerificationResults =
                expandedIpAddressRange.Select(address => _brandService.VerifyIpAddress(address, Brand.Id)).ToList();

            // *** Assert ***
            var isIpAddressRangeBlocked = ipAddressRangeVerificationResults
                .Aggregate(false, (current, result) => current || !result.Allowed);

            var isIpRegulationBlockingTypeSetToRedirection = ipAddressRangeVerificationResults
                .Aggregate(true, (current, result) => current &&
                    result.BlockingType == IpRegulationConstants.BlockingTypes.Redirection);

            var isIpRegulationRedirectionUrlSetToDefinedUrl = ipAddressRangeVerificationResults
                .Aggregate(true, (current, result) => current &&
                    result.RedirectionUrl == redirectionUrl);

            Assert.True(isIpAddressRangeBlocked);
            Assert.True(isIpRegulationBlockingTypeSetToRedirection);
            Assert.True(isIpRegulationRedirectionUrlSetToDefinedUrl);
        }

        [Test]
        public void Block_brand_website_for_ip_address_ranges_specified_with_dashes()
        {
            // *** Arrange ***
            var ipAddressRangeWithDash = TestDataGenerator.GetRandomIpAddressV4Range(IpAddressRangeSeparators.Dash);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressRangeWithDash,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // Expand ip address range into list of ip addresses, e.g. 192.168.5.10/12 is expanded into [192.168.5.10, 192.168.5.11]
            var expandedIpAddressRange = IpRegulationRangeHelper.ExpandIpAddressRange(ipAddressRangeWithDash).ToList();

            // *** Act ***
            var ipAddressRangeVerificationResults =
                expandedIpAddressRange.Select(address => _brandService.VerifyIpAddress(address, Brand.Id)).ToList();

            // *** Assert ***
            var isIpAddressRangeBlocked = ipAddressRangeVerificationResults
                .Aggregate(false, (current, result) => current || !result.Allowed);

            var isIpRegulationBlockingTypeSetToRedirection = ipAddressRangeVerificationResults
                .Aggregate(true, (current, result) => current &&
                    result.BlockingType == IpRegulationConstants.BlockingTypes.Redirection);

            var isIpRegulationRedirectionUrlSetToDefinedUrl = ipAddressRangeVerificationResults
                .Aggregate(true, (current, result) => current &&
                    result.RedirectionUrl == redirectionUrl);

            Assert.True(isIpAddressRangeBlocked);
            Assert.True(isIpRegulationBlockingTypeSetToRedirection);
            Assert.True(isIpRegulationRedirectionUrlSetToDefinedUrl);
        }

        [Test]
        public void Block_brand_website_for_ip_address_specified_in_ipv6_format()
        {
            // *** Arrange ***
            var ipAddressv6 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressv6,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // *** Act ***
            var verificationResult = _brandService.VerifyIpAddress(ipAddressv6, Brand.Id);

            // *** Assert ***
            var isIpAddressv6Blocked = !verificationResult.Allowed;
            var isIpRegulationBlockingTypeSetToRedirection = verificationResult.BlockingType ==
                                                             IpRegulationConstants.BlockingTypes.Redirection;
            var isIpRegulationRedirectionUrlSetToDefinedUrl = verificationResult.RedirectionUrl == redirectionUrl;

            Assert.True(isIpAddressv6Blocked);
            Assert.True(isIpRegulationBlockingTypeSetToRedirection);
            Assert.True(isIpRegulationRedirectionUrlSetToDefinedUrl);
        }

        [Test]
        public void Allow_access_to_brand_website_for_ip_address_specified_in_ipv6_format()
        {
            // *** Arrange ***
            var ipAddressv6 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulationData = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressv6,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulationData);

            var ipAddressNotInBlackList = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            // *** Act ***
            var verificationResult = _brandService.VerifyIpAddress(ipAddressNotInBlackList, Brand.Id);

            // *** Assert ***
            Assert.True(verificationResult.Allowed);
        }

        [Test]
        public void Allow_access_to_brand_website_for_ip_address_specified_in_ipv4_format()
        {
            // *** Arrange ***
            var ipAddressv4 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulationData = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressv4,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulationData);

            var ipAddressNotInBlackList = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            // *** Act ***
            var verificationResult = _brandService.VerifyIpAddress(ipAddressNotInBlackList, Brand.Id);

            // *** Assert ***
            Assert.True(verificationResult.Allowed);
        }

        [Test]
        public void Block_brand_website_for_ip_address_specified_in_ipv4_format()
        {
            // *** Arrange ***
            var ipAddressv4 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            const string redirectionUrl = "google.com";

            if (Brand == null)
            {
                //Assert.Fail throws the exception for NUnit framework, so execution of the test stops 
                Assert.Fail("Brand not found");
            }

            var brandIpRegulation = new AddBrandIpRegulationData
            {
                IpAddress = ipAddressv4,
                BrandId = Brand.Id,
                // Ip address is blocked with redirection to specified Url
                BlockingType = IpRegulationConstants.BlockingTypes.Redirection,
                RedirectionUrl = redirectionUrl
            };

            _brandService.CreateIpRegulation(brandIpRegulation);

            // *** Act ***
            var verificationResult = _brandService.VerifyIpAddress(ipAddressv4, Brand.Id);

            // *** Assert ***
            var isIpAddressv4Blocked = !verificationResult.Allowed;
            var isIpRegulationBlockingTypeSetToRedirection = verificationResult.BlockingType ==
                                                             IpRegulationConstants.BlockingTypes.Redirection;
            var isIpRegulationRedirectionUrlSetToDefinedUrl = verificationResult.RedirectionUrl == redirectionUrl;

            Assert.True(isIpAddressv4Blocked);
            Assert.True(isIpRegulationBlockingTypeSetToRedirection);
            Assert.True(isIpRegulationRedirectionUrlSetToDefinedUrl);
        }

        [Test]
        public void Can_access_brand_website_from_localhost_specified_in_ipv4()
        {
            // *** Arrange ***
            const string localhostIpAddressv4 = "127.0.0.1";

            // *** Act ***
            var isAccessAllowed = _brandService.VerifyIpAddress(localhostIpAddressv4).Allowed;

            // *** Assert ***
            Assert.True(isAccessAllowed);
        }

        [Test]
        public void Can_access_brand_website_from_localhost_specified_in_ipv6()
        {
            // *** Arrange ***
            const string localhostIpAddressv6Short = "::1";
            const string localhostIpAddressv6Long = "0:0:0:0:0:0:0:1";

            // *** Act ***
            var isAccessAllowedWithShortAddress = _brandService.VerifyIpAddress(localhostIpAddressv6Short).Allowed;
            var isAccessAllowedWithLongAddress = _brandService.VerifyIpAddress(localhostIpAddressv6Long).Allowed;

            // *** Assert ***
            Assert.True(isAccessAllowedWithShortAddress);
            Assert.True(isAccessAllowedWithLongAddress);
        }
    }
}
