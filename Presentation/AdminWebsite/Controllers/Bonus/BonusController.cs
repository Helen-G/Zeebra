using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class BonusController : BaseController
    {
        private readonly BonusBuilder _bonusBuilder;
        private readonly BonusManagementCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;
        private readonly UserService _userService;

        public BonusController(
            BonusManagementCommands bonusCommands,
            BonusQueries bonusQueries,
            BonusBuilder bonusBuilder,
            UserService userService)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
            _bonusBuilder = bonusBuilder;
            _userService = userService;
        }

        static BonusController()
        {
            Mapper.CreateMap<Bonus, BonusVM>()
                .ForMember(dest => dest.LicenseeName, opt => opt.MapFrom(data => data.Template.Info.Brand.LicenseeName))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(data => data.Template.Info.Brand.Name))
                .ForMember(dest => dest.ActiveFrom, opt => opt.MapFrom(data => Format.FormatDate(data.ActiveFrom, false)))
                .ForMember(dest => dest.ActiveTo, opt => opt.MapFrom(data => Format.FormatDate(data.ActiveTo, false)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(bonus => BonusQueries.GetTemplateType(bonus.Template.Info)))
                .ForMember(dest => dest.DurationDays, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Days;
                }))
                .ForMember(dest => dest.DurationHours, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Hours;
                }))
                .ForMember(dest => dest.DurationMinutes, opt => opt.ResolveUsing(data =>
                {
                    var duration = data.DurationEnd - data.ActiveFrom;
                    return duration.Minutes;
                }))
                .ForMember(dest => dest.DurationStart, opt => opt.MapFrom(data => Format.FormatDate(data.DurationStart, true)))
                .ForMember(dest => dest.DurationEnd, opt => opt.MapFrom(data => Format.FormatDate(data.DurationEnd, true)));
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var queryable = _bonusQueries.GetCurrentVersionBonuses()
                .Include(b => b.Template)
                .Include(b => b.Template.Info.Brand);

            var dataBuilder = new SearchPackageDataBuilder<Bonus>(searchPackage, queryable);

            var data = dataBuilder
                .Map(bonus => bonus.Id, bonus => MapBonusCell(bonus))
                .GetPageData(x => x.Name);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ChangeStatus(ToggleBonusStatusVM model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                return Json(new { Success = false, validationResult.Errors });

            _bonusCommands.ChangeBonusStatus(model);
            return Json(new { Success = true });
        }

        public ActionResult GetRelatedData(Guid? id)
        {
            BonusVM bonusVm = null;

            if (id.HasValue)
            {
                var bonus = _bonusQueries.GetCurrentVersionBonuses().Single(a => a.Id == id);
                bonusVm = Mapper.Map<BonusVM>(bonus);
            }

            var useBrandFilter = !id.HasValue;
            var templates = GetTemplatesList(useBrandFilter);

            return Json(new
            {
                bonus = bonusVm,
                templates
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateUpdate(BonusVM data)
        {
            var buildResult = _bonusBuilder.BuildBonus(data);
            if (buildResult.IsValid)
            {
                var bonus = buildResult.Entity;
                if (data.Id == Guid.Empty)
                {
                    _bonusCommands.AddBonus(bonus);
                }
                else
                {
                    _bonusCommands.UpdateBonus(bonus);
                }
                return Json(new { Success = true, BonusId = bonus.Id });
            }

            return Json(new { Success = false, buildResult.Errors });
        }

        private object MapBonusCell(Bonus bonus)
        {
            return new object[]
            {
                bonus.Name,
                bonus.Code,
                BonusQueries.GetTemplateType(bonus.Template.Info),
                bonus.Template.Info.Mode.ToString(),
                Format.FormatDate(bonus.ActiveFrom, false),
                Format.FormatDate(bonus.ActiveTo, false),
                bonus.Template.Info.Brand.LicenseeName,
                bonus.Template.Info.Brand.Name,
                "", //Product goes here
                bonus.IsActive,
                bonus.CreatedBy,
                Format.FormatDate(bonus.CreatedOn),
                bonus.UpdatedBy,
                Format.FormatDate(bonus.UpdatedOn),
                bonus.Description
            };
        }
        private IEnumerable<object> GetTemplatesList(bool useBrandFilter)
        {
            var templates = _bonusQueries.GetCurrentVersionTemplates()
                .Where(template => template.Status == TemplateStatus.Complete);

            if (useBrandFilter)
            {
                var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

                templates = templates.Where(x => brandFilterSelections.Contains(x.Info.Brand.Id));
            }

            foreach (var template in templates.OrderBy(x => x.Info.Name).ToArray())
            {
                yield return new
                {
                    template.Id,
                    template.Info.Name,
                    brand = new
                    {
                        template.Info.Brand.Id,
                        template.Info.Brand.Name,
                        template.Info.Brand.LicenseeName
                    },
                    RequireBonusCode = template.Info.Mode == IssuanceMode.AutomaticWithCode
                };
            }
        }
    }
}