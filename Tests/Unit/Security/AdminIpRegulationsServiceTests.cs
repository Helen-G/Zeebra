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
using AutoMapper;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class AdminIpRegulationsServiceTests : SecurityTestsBase
    {
        private IpRegulationServiceBase _serviceBase;
        private BackendIpRegulationService _backendService;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _serviceBase = Container.Resolve<IpRegulationServiceBase>();
            _backendService = Container.Resolve<BackendIpRegulationService>();
        }

        [Test]
        public void Can_create_admin_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            /*** Act ***/
            _backendService.CreateIpRegulation(adminIpRegulation);

            /*** Assert ***/
            var regulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            Assert.IsNotNull(regulation);
        }

        [Test]
        public void Cannot_create_admin_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to create new IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Add, Modules.BackendIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions:sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to create ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BackendIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            /*** Act ***/
            // Signin user that has sufficient permissions to create new backend IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method CreateIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _backendService.CreateIpRegulation(adminIpRegulation));


            // Signin user that has insufficient permissions to create new backend IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method CreateIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _backendService.CreateIpRegulation(adminIpRegulation));
        }

        [Test]
        public void Can_update_admin_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            var regulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            var editIpAddress = TestDataGenerator.GetRandomIpAddress();
            var editDescription = TestDataGenerator.GetRandomString(20);

            regulation.IpAddress = editIpAddress;
            regulation.Description = editDescription;

            var updateRegulationData = Mapper.DynamicMap<EditBackendIpRegulationData>(regulation);

            /*** Act ***/
            _backendService.UpdateIpRegulation(updateRegulationData);

            /*** Assert ***/
            var updatedRegulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == editIpAddress);

            Assert.IsNotNull(updatedRegulation);
            Assert.AreEqual(updatedRegulation.IpAddress, editIpAddress);
            Assert.AreEqual(updatedRegulation.Description, editDescription);
        }

        [Test]
        public void Cannot_update_admin_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to update IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Edit, Modules.BackendIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions: sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to update ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BackendIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            var regulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            var editIpAddress = TestDataGenerator.GetRandomIpAddress();
            var editDescription = TestDataGenerator.GetRandomString(20);

            regulation.IpAddress = editIpAddress;
            regulation.Description = editDescription;

            var updateRegulationData = Mapper.DynamicMap<EditBackendIpRegulationData>(regulation);

            /*** Act ***/
            // Signin user that has sufficient permissions to update backend IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method UpdateIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _backendService.UpdateIpRegulation(updateRegulationData));


            // Signin user that has insufficient permissions to update backend IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method UpdateIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _backendService.UpdateIpRegulation(updateRegulationData));
        }

        [Test]
        public void Can_delete_admin_ip_regulation()
        {
            /*** Arrange ***/
            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            var regulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            /*** Act ***/
            _backendService.DeleteIpRegulation(regulation.Id);

            /*** Assert ***/
            var deletedRegulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);
            Assert.IsNull(deletedRegulation);

        }

        [Test]
        public void Cannot_delete_admin_ip_regulation_without_permissions()
        {
            /*** Arrange ***/
            // Create role and user that has permission to delete IP regulation
            var sufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.Delete, Modules.BackendIpRegulationManager)
            };
            var roleWithPermission = CreateTestRole(permissions: sufficientPermissions);
            var userWithPermission = CreateTestUser(roleWithPermission.Id);

            // Create role and user that has permission only to view IP regulations not to delete ones
            var insufficientPermissions = new List<Core.Security.Data.Permission>
            {
                PermissionService.GetPermission(Permissions.View, Modules.BackendIpRegulationManager)
            };
            var roleWithoutPermission = CreateTestRole(permissions: insufficientPermissions);
            var userWithoutPermission = CreateTestUser(roleWithoutPermission.Id);

            var ipAddress = TestDataGenerator.GetRandomIpAddress();

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddress
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            var regulation = _backendService.GetIpRegulations().SingleOrDefault(ip => ip.IpAddress == ipAddress);

            /*** Act ***/
            // Signin user that has sufficient permissions to delete backend IP regulation 
            UserService.SignInUser(userWithPermission);
            // Check that method DeleteIpRegulation doesn't throw InsufficientPermissionsException 
            Assert.DoesNotThrow(
                () => _backendService.DeleteIpRegulation(regulation.Id));

            // Signin user that has insufficient permissions to delete backend IP regulation
            UserService.SignInUser(userWithoutPermission);
            // Check that method DeleteIpRegulation throws InsufficientPermissionsException 
            Assert.Throws<InsufficientPermissionsException>(
                () => _backendService.DeleteIpRegulation(regulation.Id));
        }


        [Test]
        public void Can_expand_ip_address_range()
        {
            // *** Arrange ***
            const string testAddressRangeLastSegment = "192.168.5.10-20";
            const string testAddressRangeAllSegmnents = "160-170.100-120.5-10.10-20";
            const string testAddressWithoutRange = "192.168.5.10";

            // *** Act ***
            var expandedRangeInLastSegment = IpRegulationRangeHelper.ExpandIpAddressRange(testAddressRangeLastSegment).ToList();
            var expandedRangeInAllSegments = IpRegulationRangeHelper.ExpandIpAddressRange(testAddressRangeAllSegmnents).ToList();
            var expandedAddressWithoutRange = IpRegulationRangeHelper.ExpandIpAddressRange(testAddressWithoutRange).ToList();

            // *** Assert ***
            Assert.True(expandedRangeInLastSegment.Count() == 11);
            Assert.True(expandedRangeInLastSegment.First() == "192.168.5.10");
            Assert.True(expandedRangeInLastSegment.Last() == "192.168.5.20");

            Assert.True(expandedRangeInAllSegments.Count() == 15246);
            Assert.True(expandedRangeInAllSegments.First() == "160.100.5.10");
            Assert.True(expandedRangeInAllSegments.Last() == "170.120.10.20");

            Assert.True(expandedAddressWithoutRange.Count() == 1);
            Assert.True(expandedAddressWithoutRange.First() == testAddressWithoutRange);
        }

        [Test]
        public void Can_verify_unique_ip_address_v4_for_admin_website()
        {
            // *** Arrange ***
            const string ipAddressRange = "192.168.5.10-20";

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressRange
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // *** Act ***
            var isIpAddressWithinRegulationRange = _backendService.IsIpAddressUnique("192.168.5.15");
            var isIpAddressOutOfRegulationRange = _backendService.IsIpAddressUnique("192.168.5.52");
            var isIpAddressRangeIntersectRegulationRange = _backendService.IsIpAddressUnique("192.168.5.15-25");
            var isIpAddressRangeDontIntersectRegulationRange = _backendService.IsIpAddressUnique("192.168.5.25-30");

            // *** Assert ***
            Assert.False(isIpAddressWithinRegulationRange);
            Assert.True(isIpAddressOutOfRegulationRange);
            Assert.False(isIpAddressRangeIntersectRegulationRange);
            Assert.True(isIpAddressRangeDontIntersectRegulationRange);
        }

        [Test]
        public void Can_verify_loopback_ip_address()
        {
            var localhostAliases = new[] { "127.0.0.1", "::1", "0:0:0:0:0:0:0:1" };

            var isAllAddressesLoopback = localhostAliases.Select(a => _backendService.IsLocalhost(a))
                .Aggregate(true, (current, result) => current && result);

            Assert.True(isAllAddressesLoopback);
        }

        [Test]
        public void Can_verify_unique_ip_address_v6_for_admin_website()
        {
            // *** Arrange ***
            const string ipAddressRange = "1111::6666:7777:8888/32";

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressRange
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // *** Act ***
            var isIpAddressWithinRegulationRange = _backendService.IsIpAddressUnique("1111:0000:0000:0000:0000:6666:7777:9999");
            var isIpAddressOutOfRegulationRange = _backendService.IsIpAddressUnique("1111:0000:0000:0000:0000:6666:7777:7777");
            var isIpAddressRangeIntersectRegulationRange = _backendService.IsIpAddressUnique("1111::6666:7777:8888/24");
            var isIpAddressRangeDontIntersectRegulationRange = _backendService.IsIpAddressUnique("1111::6666:7777:1111/128");

            // *** Assert ***
            Assert.False(isIpAddressWithinRegulationRange);
            Assert.True(isIpAddressOutOfRegulationRange);
            Assert.False(isIpAddressRangeIntersectRegulationRange);
            Assert.True(isIpAddressRangeDontIntersectRegulationRange);
        }

        [Test]
        public void Can_verify_ip_address_range_lower_and_upper_bounds_for_admin_website()
        {
            // *** Arrange ***
            const string ipAddressRange = "192.168.5.10-20";

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressRange
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // *** Act ***
            var isLowerBoundAllowed = _backendService.VerifyIpAddress("192.168.5.10");
            var isUpperBoundAllowed = _backendService.VerifyIpAddress("192.168.5.20");

            // *** Assert ***
            Assert.True(isLowerBoundAllowed);
            Assert.True(isUpperBoundAllowed);
        }

        public static class IpAddressRangeSeparators
        {
            public static readonly string Slash = "/";
            public static readonly string Dash = "-";
        }

        [Test]
        public void Allow_access_to_admin_website_for_ip_address_ranges_specified_with_slashes()
        {
            // *** Arrange ***
            var ipAddressRangeWithSlash = TestDataGenerator.GetRandomIpAddressV4Range(IpAddressRangeSeparators.Slash);

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressRangeWithSlash
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // Expand ip address range into list of ip addresses
            var expandedIpAddressRange = IpRegulationRangeHelper.ExpandIpAddressRange(ipAddressRangeWithSlash).ToList();

            // *** Act ***
            var isIpAddressRangeAllowed = expandedIpAddressRange.Select(address => _backendService.VerifyIpAddress(address))
                .Aggregate(true, (current, result) => current && result);

            // *** Assert ***
            Assert.True(isIpAddressRangeAllowed);
        }

        [Test]
        public void Allow_access_to_admin_website_for_ip_address_ranges_specified_with_dashes()
        {
            // *** Arrange ***
            var ipAddressRangeWithDash = TestDataGenerator.GetRandomIpAddressV4Range(IpAddressRangeSeparators.Dash);

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressRangeWithDash
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // Expand ip address range into list of ip addresses, e.g. 192.168.5.10/12 is expanded into [192.168.5.10, 192.168.5.11]
            var expandedIpAddressRange = IpRegulationRangeHelper.ExpandIpAddressRange(ipAddressRangeWithDash).ToList();

            // *** Act ***
            var isIpAddressRangeAllowed = expandedIpAddressRange.Select(address => _backendService.VerifyIpAddress(address))
                .Aggregate(true, (current, result) => current && result);

            // *** Assert ***
            Assert.True(isIpAddressRangeAllowed);
        }

        [Test]
        public void Allow_access_to_admin_website_for_ip_address_specified_in_ipv4_format()
        {
            // *** Arrange ***
            var ipAddressv4 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            var adminIpRegulationData = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressv4
            };

            _backendService.CreateIpRegulation(adminIpRegulationData);

            // *** Act ***
            var isIpAddressAllowed = _backendService.VerifyIpAddress(ipAddressv4);

            // *** Assert ***
            Assert.True(isIpAddressAllowed);
        }

        [Test]
        public void Allow_access_to_admin_website_for_ip_address_specified_in_ipv6_format()
        {
            // *** Arrange ***
            var ipAddressv6 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            var adminIpRegulationData = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressv6
            };

            _backendService.CreateIpRegulation(adminIpRegulationData);

            // *** Act ***
            var isIpAddressAllowed = _backendService.VerifyIpAddress(ipAddressv6);

            // *** Assert ***
            Assert.True(isIpAddressAllowed);
        }

        [Test]
        public void Cannot_access_admin_website_with_ip_address_specified_in_ipv4_format()
        {
            // *** Arrange ***
            var ipAddressv4 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            var adminIpRegulationData = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressv4
            };

            _backendService.CreateIpRegulation(adminIpRegulationData);

            var ipAddressNotInWhiteList = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            // *** Act ***
            var isIpAddressBlocked = _backendService.VerifyIpAddress(ipAddressNotInWhiteList);

            // *** Assert ***
            Assert.True(isIpAddressBlocked);
        }

        [Test]
        public void Cannot_access_admin_website_with_ip_address_specified_in_ipv6_format()
        {
            // *** Arrange ***
            var ipAddressv6 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            var adminIpRegulationData = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressv6
            };

            _backendService.CreateIpRegulation(adminIpRegulationData);

            var ipAddressNotInWhiteList = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6);

            // *** Act ***
            var isIpAddressBlocked = _backendService.VerifyIpAddress(ipAddressNotInWhiteList);

            // *** Assert ***
            Assert.True(isIpAddressBlocked);
        }

        [Test]
        public void Cannot_access_admin_website_for_single_ip_address_specified_in_ipv4_format()
        {
            // *** Arrange ***
            var ipAddressv4 = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv4);

            var adminIpRegulation = new AddBackendIpRegulationData
            {
                IpAddress = ipAddressv4
            };

            _backendService.CreateIpRegulation(adminIpRegulation);

            // *** Act ***
            var isIpAddressAllowed = _backendService.VerifyIpAddress(ipAddressv4);

            // *** Assert ***
            Assert.True(isIpAddressAllowed);
        }

        [Test]
        public void Can_access_admin_website_from_localhost_specified_in_ipv4()
        {
            // *** Arrange ***
            const string localhostIpAddressv4 = "127.0.0.1";

            // *** Act ***
            var isAccessAllowed = _backendService.VerifyIpAddress(localhostIpAddressv4);

            // *** Assert ***
            Assert.True(isAccessAllowed);
        }

        [Test]
        public void Can_access_admin_website_from_localhost_specified_in_ipv6()
        {
            // *** Arrange ***
            const string localhostIpAddressv6Short = "::1";
            const string localhostIpAddressv6Long = "0:0:0:0:0:0:0:1";

            // *** Act ***
            var isAccessAllowedWithShortAddress = _backendService.VerifyIpAddress(localhostIpAddressv6Short);
            var isAccessAllowedWithLongAddress = _backendService.VerifyIpAddress(localhostIpAddressv6Long);

            // *** Assert ***
            Assert.True(isAccessAllowedWithShortAddress);
            Assert.True(isAccessAllowedWithLongAddress);
        }
    }
}
