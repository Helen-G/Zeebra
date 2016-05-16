using System;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AutoMapper;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class RoleServiceTests : SecurityTestsBase
    {
        [Test]
        public void Can_create_role()
        {
            var role = CreateTestRole();

            Assert.IsNotNull(role);
            Assert.False(role.Id == Guid.Empty);
        }

        [Test]
        public void Can_update_role()
        {
            // *** Arrange ***
            var role = CreateTestRole();
            var user = CreateTestUser(role.Id);

            role.Code = TestDataGenerator.GetRandomString();
            role.Name = TestDataGenerator.GetRandomString();
            role.Description = TestDataGenerator.GetRandomString();

            var roleData = Mapper.DynamicMap<EditRoleData>(role);

            UserService.SignInUser(user);

            // *** Act ***
            RoleService.UpdateRole(roleData);

            // *** Assert ***
            var updatedRole = RoleService.GetRoleById(role.Id, true);

            Assert.True(updatedRole.Code == role.Code);
            Assert.True(updatedRole.Name == role.Name);
            Assert.True(updatedRole.Description == role.Description);
            Assert.True(updatedRole.UpdatedBy.Id == user.Id);
            Assert.True(updatedRole.UpdatedDate.HasValue);
        }
    }
}
