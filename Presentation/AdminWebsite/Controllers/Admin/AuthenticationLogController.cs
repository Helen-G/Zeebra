using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Report.Data.Admin;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AuthenticationLogController : BaseController
    {
        private readonly ReportQueries _reportQueries;
        private readonly UserService _userService;

        public AuthenticationLogController(
            ReportQueries reportQueries, 
            UserService userService)
        {
            _reportQueries = reportQueries;
            _userService = userService;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult AdminAuthData(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<AdminAuthenticationLog>(searchPackage, _reportQueries.GetAdminAuthenticationLog());
            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.DatePerformed,
                    r.PerformedBy,
                    r.IPAddress,
                    r.Success,
                    r.FailReason,
                    r.Headers
                })
                .GetPageData(r => r.DatePerformed);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        [SearchPackageFilter("searchPackage")]
        public ActionResult MemberAuthData(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);
            var logs = _reportQueries.GetMemberAuthenticationLog().Where(x => brandFilterSelections.Contains(x.BrandId));
            var dataBuilder = new SearchPackageDataBuilder<MemberAuthenticationLog>(searchPackage, logs);

            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Brand,
                    r.DatePerformed,
                    r.PerformedBy,
                    r.IPAddress,
                    r.Success,
                    r.FailReason,
                    r.Headers
                })
                .GetPageData(r => r.DatePerformed);

            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}