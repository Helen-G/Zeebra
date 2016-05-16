using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Report;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Report.Data.Admin;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class AdminActivityLogController : BaseController
    {
        private readonly ReportQueries _reportQueries;

        public AdminActivityLogController(
            ReportQueries reportQueries
            )
        {
            _reportQueries = reportQueries;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var dataBuilder = new SearchPackageDataBuilder<AdminActivityLog>(searchPackage, _reportQueries.GetAdminActivityLog());
            var data = dataBuilder
                .Map(r => r.Id, r => new object[]
                {
                    r.Category.ToString(),
                    r.ActivityDone,
                    r.PerformedBy,
                    r.DatePerformed.DateTime.GetNormalizedDateTime(),
                    r.Remarks
                })
                .GetPageData(r => r.DatePerformed);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


    }
}