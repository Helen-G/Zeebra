using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class RedemptionController : BaseController
    {
        private readonly BonusQueries _bonusQueries;
        private readonly BonusCommands _bonusCommands;
        private readonly UserService _userService;

        static RedemptionController()
        {
            Mapper.CreateMap<BonusRedemption, BonusRedemptionVM>()
                .ForMember(ui => ui.Username, opt => opt.MapFrom(br => br.Player.Name))
                .ForMember(ui => ui.LicenseeName, opt => opt.MapFrom(br => br.Player.Brand.LicenseeName))
                .ForMember(ui => ui.BrandName, opt => opt.MapFrom(br => br.Player.Brand.Name))
                .ForMember(ui => ui.BonusName, opt => opt.MapFrom(br => br.Bonus.Name));
        }

        public RedemptionController(BonusQueries bonusQueries, BonusCommands bonusCommands, UserService userService)
        {
            _bonusQueries = bonusQueries;
            _bonusCommands = bonusCommands;
            _userService = userService;
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var queryable = _bonusQueries.GetBonusRedemptions()
                .Where(x => brandFilterSelections.Contains(x.Player.Brand.Id));

            var dataBuilder = new SearchPackageDataBuilder<BonusRedemption>(searchPackage, queryable);

            var data = dataBuilder
                .Map(redemption => redemption.Id, redemption => MapGridCell(redemption))
                .GetPageData(x => x.Player.Name);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get(Guid? playerId, Guid? redemptionId)
        {
            BonusRedemptionVM bonusRedemptionVm = null;
            if (playerId.HasValue && redemptionId.HasValue)
            {
                var redemption = _bonusQueries.GetBonusRedemption(playerId.Value, redemptionId.Value);
                bonusRedemptionVm = Mapper.Map<BonusRedemptionVM>(redemption);
            }

            return Json(new
            {
                redemption = bonusRedemptionVm
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Cancel(Guid playerId, Guid redemptionId)
        {
            try
            {
                _bonusCommands.CancelBonusRedemption(playerId, redemptionId);
                return Json(new { Success = true });
            }
            catch (RegoException exception)
            {
                return Json(new { Success = false, exception.Message });
            }
        }

        private object MapGridCell(BonusRedemption redemption)
        {
            return new object[]
            {
                redemption.Id,
                redemption.Player.Id,
                redemption.Player.Name,
                redemption.Bonus.Name,
                redemption.Player.Brand.LicenseeName,
                redemption.Player.Brand.Name,
                redemption.Amount,
                redemption.ActivationState,
                redemption.Rollover,
                redemption.LockedAmount,
                redemption.RolloverState,
                Format.FormatDate(redemption.CreatedOn),
                Format.FormatDate(redemption.UpdatedOn)
            };
        }
    }
}