using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.ViewModels;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;
using AFT.RegoV2.Domain.Security;
using AFT.RegoV2.Shared.Constants;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Token = CurrentUser.Token;
            ViewBag.RefreshToken = CurrentUser.RefreshToken;

            return View();
        }

        [HttpPost]
        public string GetSecurityData()
        {
            var service = DependencyResolver.Current.GetService<UserService>();
            var permissions = DependencyResolver.Current.GetService<PermissionService>();
            var user = service.GetUserById(CurrentUser.UserId);

            var viewModel = new SecurityViewModel
            {
                UserName = CurrentUser.UserName,
                Operations = permissions.GetPermissions(),
                UserPermissions = user.Role.Permissions.Select(p => p.PermissionId),
                Licensees = user.Licensees.Select(l => l.Id),
                Permissions = ConstantsHelper.GetConstantsDictionary<Permissions>(),
                Categories = ConstantsHelper.GetConstantsDictionary<Modules>(),
                IsSingleBrand = user.AllowedBrands.Count == 1,
                IsSuperAdmin = user.Role.Id == RoleIds.SuperAdminId
            };

            return SerializeJson(viewModel);
        }
    }
}