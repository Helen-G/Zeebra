using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared.Constants;
using Brand = AFT.RegoV2.Core.Brand.Data.Brand;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class SecurityTestHelper
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly ISecurityRepository _repository;
        private readonly PermissionService _permissionService;
        private readonly RoleService _roleService;
        private readonly UserService _userService;
        private readonly BrandQueries _brandQueries;

        public AuthUser CurrentUser { get { return _securityProvider.User; } }

        public SecurityTestHelper(
            ISecurityProvider securityProvider,
            ISecurityRepository repository,
            PermissionService permissionService,
            RoleService roleService,
            UserService userService,
            BrandQueries brandQueries)
        {
            _securityProvider = securityProvider;
            _repository = repository;
            _permissionService = permissionService;
            _roleService = roleService;
            _userService = userService;
            _brandQueries = brandQueries;
        }

        //todo: we have to remove this ASAP
        public void SignInUser()
        {
            var user = new User
            {
                Role = new Role
                {
                    Id = RoleIds.SuperAdminId
                }
            };
            if (!_repository.Users.Any(u => u.Id == user.Id))
            {
                _repository.Users.Add(user);
            }

            //todo: this command should be a part of Security Sub-domain
            _userService.SignInUser(user);
        }

        public void SignInUser(User user)
        {
            _userService.SignInUser(user);
        }

        public void SignInSuperAdmin()
        {
            var superAdmin = _userService.GetUserById(RoleIds.SuperAdminId);
            SignInUser(superAdmin);
        }

        public void PopulatePermissions()
        {
            var seed = new SecuritySeed(_repository);
            seed.PopulatePermissions();
            seed.RegisterPermissions();
        }

        public Role CreateRole(IEnumerable<Guid> licenseeIds, IList<Permission> permissions = null)
        {
            var roleData = new AddRoleData
            {
                Code = "Role-" + TestDataGenerator.GetRandomString(5),
                Name = "Role-" + TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(),
                CheckedPermissions = permissions != null ? permissions.Select(p => p.Id).ToList() : _permissionService.GetPermissions().Select(o => o.Id).ToList(),
                AssignedLicensees = licenseeIds.ToList()
            };

            return _roleService.CreateRole(roleData);
        }

        public Role CreateTestRole(Guid? createdBy = null, ICollection<Permission> operations = null)
        {
            var role = new AddRoleData
            {
                Code = "Role-" + TestDataGenerator.GetRandomString(5),
                Name = "Role-" + TestDataGenerator.GetRandomString(5),
                Description = TestDataGenerator.GetRandomString(),
                CheckedPermissions = (operations != null ? operations.Select(p => p.Id)
                    : _permissionService.GetPermissions().Select(o => o.Id)).ToList()
            };

            if (createdBy.HasValue)
            {
                var user = _userService.GetUserById(createdBy.Value);
                _userService.SignInUser(user);
            }

            return _roleService.CreateRole(role);
        }

        public User CreateUser(Guid licenseeId, IEnumerable<Brand> brands = null, IEnumerable<string> currencies = null, string password = null, Guid? roleId = null)
        {
            var licenseeIds = new[] { licenseeId };

            return CreateUser(licenseeIds, brands, currencies, password, roleId);
        }

        public User CreateUser(IEnumerable<Guid> licenseeIds, IEnumerable<Brand> brands = null, IEnumerable<string> currencies = null, string password = null, Guid? roleId = null)
        {
            var userName = "User-" + TestDataGenerator.GetRandomString(5);

            if (password == null)
                password = TestDataGenerator.GetRandomString();

            if (roleId == null)
            {
                var role = CreateRole(licenseeIds);
                roleId = role.Id;
            }

            licenseeIds = licenseeIds ?? _brandQueries.GetLicensees().Select(l => l.Id);
            brands = brands ?? _brandQueries.GetBrands();
            currencies = currencies ?? _brandQueries.GetCurrencies().Select(c => c.Code);

            var userData = new AddUserData
            {
                Username = userName,
                FirstName = userName,
                LastName = userName,
                Password = password,
                Language = "English",
                Status = UserStatus.Active,
                AssignedLicensees = licenseeIds.ToList(),
                AllowedBrands = brands.Select(b => b.Id).ToList(),
                Currencies = currencies.ToList(),
                RoleId = roleId
            };

            return _userService.CreateUser(userData);
        }

        public User CreateUserWithPermissions(string category, string[] permissions, IEnumerable<Brand> brands = null, string password = null)
        {
            // Create role and user that have provided permissions
            var insufficientPermissions = permissions.Select(
                insuffPermission => _permissionService.GetPermissions().First(p => p.Name == insuffPermission && p.Module == category)).ToList();

            var roleWithoutPermission = CreateTestRole(operations: insufficientPermissions);
            var userWithoutPermission = CreateUser(Guid.NewGuid(), brands, password: password, roleId: roleWithoutPermission.Id);

            return userWithoutPermission;
        }
    }
}