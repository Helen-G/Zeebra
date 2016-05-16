using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Data.Bonus;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class PlayerBonusController : BaseController
    {
        private readonly BonusQueries _bonusQueries;
        private readonly BonusCommands _bonusCommands;

        public PlayerBonusController(BonusQueries bonusQueries, BonusCommands bonusCommands)
        {
            _bonusQueries = bonusQueries;
            _bonusCommands = bonusCommands;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage, Guid playerId)
        {
            var dataBuilder = new SearchPackageDataBuilder<ManualByCsQualifiedBonus>(searchPackage, _bonusQueries.GetManualByCsQualifiedBonuses(playerId).AsQueryable());
            var data = dataBuilder
                .Map(b => b.Id, b => new object[]
                {
                    b.Name,
                    b.Code,
                    b.Type,
                    b.Status,
                    b.Description
                })
                .GetPageData(r => r.Name);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Transactions(Guid playerId, Guid bonusId)
        {
            var qualifiedTransactions = _bonusQueries.GetManualByCsQualifiedTransactions(playerId, bonusId);

            return Json(qualifiedTransactions
                .OrderByDescending(qt => qt.Date)
                .Select(qt => new
                {
                    qt.Id, qt.Amount, qt.BonusAmount,
                    qt.CurrencyCode,
                    Date = Format.FormatDate(qt.Date)
                }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult IssueBonus(IssueBonusByCsVM model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                return Json(new { Success = false, Errors = validationResult.Errors.Select(e => e.ErrorMessage) });

            _bonusCommands.IssueBonusByCs(model);
            return Json(new { Success = true });
        }
    }
}