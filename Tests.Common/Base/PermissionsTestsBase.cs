using System.Collections.Generic;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Entities;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Tests.Common.Base
{
    public abstract class PermissionsTestsBase : AdminWebsiteUnitTestsBase
    {
        protected UserService UserService;
        protected SecurityTestHelper SecurityTestHelper;
        protected BrandTestHelper BrandTestHelper;
        protected GamesTestHelper GamesTestHelper;

        public override void BeforeEach()
        {
            base.BeforeEach();

            UserService = Container.Resolve<UserService>();
            SecurityTestHelper = Container.Resolve<SecurityTestHelper>();
            BrandTestHelper = Container.Resolve<BrandTestHelper>();
            GamesTestHelper = Container.Resolve<GamesTestHelper>();
            SecurityTestHelper.PopulatePermissions();

            SecurityTestHelper.SignInUser();
        }

        protected User CreateUserWithPermissions(string category, string[] permissions)
        {
            var licensee = BrandTestHelper.CreateLicensee();
            var brand = BrandTestHelper.CreateBrand(licensee, isActive: true);
            var brands = new[] { brand };

            return SecurityTestHelper.CreateUserWithPermissions(category, permissions, brands);
        }

        protected void LogWithNewUser(string category, string permission)
        {
            var user = CreateUserWithPermissions(category, new[] { permission });
            UserService.SignInUser(user);
        }
    }
}
