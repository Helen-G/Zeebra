using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.DomainServices;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Content;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;
using TemplateInfo = AFT.RegoV2.Core.Bonus.Data.TemplateInfo;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    [Authorize]
    public class BonusTemplateController : BaseController
    {
        private readonly BonusBuilder _bonusBuilder;
        private readonly BonusManagementCommands _bonusCommands;
        private readonly BonusQueries _bonusQueries;
        private readonly IContentRepository _contentRepository;
        private readonly BrandQueries _brandQueries;
        private readonly UserService _userService;
        private readonly IGameQueries _gameQueries;
        private readonly IRiskLevelQueries _riskLevelQueries;

        public BonusTemplateController(
            BonusManagementCommands bonusCommands,
            BonusQueries bonusQueries,
            BonusBuilder bonusBuilder,
            IContentRepository content,
            BrandQueries brandQueries,
            UserService userService,
            IGameQueries gameQueries,
            IRiskLevelQueries riskLevelQueries)
        {
            _bonusCommands = bonusCommands;
            _bonusQueries = bonusQueries;
            _bonusBuilder = bonusBuilder;
            _contentRepository = content;
            _brandQueries = brandQueries;
            _userService = userService;
            _gameQueries = gameQueries;
            _riskLevelQueries = riskLevelQueries;
        }

        static BonusTemplateController()
        {
            Mapper.CreateMap<TemplateAvailability, TemplateAvailabilityVM>()
                .ForMember(dest => dest.VipLevels, opt => opt.MapFrom(data => data.VipLevels.Select(a => a.Code)))
                .ForMember(dest => dest.ExcludeBonuses,
                    opt => opt.MapFrom(data => data.ExcludeBonuses.Select(a => a.ExcludedBonusId)))
                .ForMember(dest => dest.ExcludeRiskLevels,
                    opt => opt.MapFrom(data => data.ExcludeRiskLevels.Select(a => a.ExcludedRiskLevelId)))
                .ForMember(dest => dest.PlayerRegistrationDateFrom,
                    opt => opt.MapFrom(data => Format.FormatDate(data.PlayerRegistrationDateFrom, false)))
                .ForMember(dest => dest.PlayerRegistrationDateTo,
                    opt => opt.MapFrom(data => Format.FormatDate(data.PlayerRegistrationDateTo, false)));
            Mapper.CreateMap<TemplateInfo, TemplateInfoVM>()
                .ForMember(dest => dest.TemplateType, opt => opt.ResolveUsing(BonusQueries.GetTemplateType))
                .ForMember(dest => dest.LicenseeId, opt => opt.MapFrom(data => data.Brand.LicenseeId));
            Mapper.CreateMap<TemplateRules, TemplateRulesVM>()
                .ForMember(dest => dest.FundInWallets, opt => opt.MapFrom(data => data.FundInWallets.Select(a => a.WalletId)));
            Mapper.CreateMap<TemplateWagering, TemplateWageringVM>()
                .ForMember(dest => dest.GameContributions, opt => 
                    opt.MapFrom(data => data.GameContributions.Select(c => 
                                    new GameContributionVM { GameId = c.GameId, Contribution = (c.Contribution * 100) })));
            Mapper.CreateMap<RewardTier, RewardTierVM>()
                .ForMember(dest => dest.BonusTiers, opt => opt.MapFrom(data => new List<TemplateTierVM>()));
            Mapper.CreateMap<Template, TemplateVM>()
                .AfterMap((template, data) =>
                {
                    if (template.Rules != null)
                    {
                        var inRewardTiers = template.Rules.RewardTiers;
                        if (template.Info.DepositKind == DepositKind.High)
                        {
                            foreach (var rewardTier in inRewardTiers)
                            {
                                var outRewardTier = data.Rules.RewardTiers.Single(t => t.CurrencyCode == rewardTier.CurrencyCode);

                                rewardTier.HighDepositTiers.OrderBy(dt => dt.DateCreated).ForEach(tier => outRewardTier.BonusTiers.Add(new TemplateTierVM
                                {
                                    DateCreated = tier.DateCreated,
                                    Reward = tier.Reward,
                                    From = tier.From,
                                    NotificationPercentThreshold = tier.NotificationPercentThreshold * 100
                                }));
                            }
                        }
                        else
                        {
                            foreach (var rewardTier in inRewardTiers)
                            {
                                var outRewardTier = data.Rules.RewardTiers.Single(t => t.CurrencyCode == rewardTier.CurrencyCode);

                                rewardTier.Tiers.OrderBy(t => t.DateCreated).ForEach(tier => outRewardTier.BonusTiers.Add(new TemplateTierVM
                                {
                                    DateCreated = tier.DateCreated,
                                    From = tier.From,
                                    To = tier.To,
                                    Reward = tier.Reward,
                                    MaxAmount = tier.MaxAmount
                                }));
                                if (template.Rules.RewardType == BonusRewardType.Percentage ||
                                    template.Rules.RewardType == BonusRewardType.TieredPercentage)
                                {
                                    outRewardTier.BonusTiers.ForEach(t => t.Reward = (t.Reward * 100));
                                }
                            }
                        }
                    }
                });
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var queryable = _bonusQueries
                .GetCurrentVersionTemplates()
                .Include(t => t.Info)
                .Include(t => t.Info.Brand)
                .Where(t => brandFilterSelections.Contains(t.Info.Brand.Id));

            var dataBuilder = new SearchPackageDataBuilder<Template>(searchPackage, queryable);

            var bonuses = _bonusQueries
                .GetCurrentVersionBonuses()
                .Include(b => b.Template)
                .ToList();

            var data = dataBuilder
                .Map(template => template.Id, template => MapTemplateCell(template, bonuses))
                .GetPageData(x => x.Info.Name);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRiskLevels(Guid? brandId)
        {
            if (brandId == null)
                return null;
            var riskLevels =_riskLevelQueries.GetByBrand(brandId.Value)
                .Select(riskLevel => new { riskLevel.Id, riskLevel.Name })
                .OrderBy(riskLevel => riskLevel.Name);
            return Json(new
            {
                riskLevels
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRelatedData(Guid? id)
        {
            TemplateVM templateVm = null;
            if (id.HasValue)
            {
                var template = _bonusQueries.GetCurrentVersionTemplates().Single(t => t.Id == id);
                templateVm = Mapper.Map<TemplateVM>(template);
            }

            var notificationTemplates = _contentRepository.MessageTemplates
                .OrderBy(t => t.TemplateName)
                .Select(t => new { t.Id, Name = t.TemplateName, Type = t.MessageDeliveryMethod })
                .ToList();

            var allLicensees = _brandQueries.GetLicensees()
                .Include(l => l.Brands.Select(b => b.VipLevels))
                .Include(l => l.Brands.Select(b => b.BrandCurrencies.Select(bc => bc.Currency)))
                .Include(l => l.Brands.Select(b => b.WalletTemplates))
                .Include(l => l.Brands.Select(b => b.Products))
                .ToList();

            var licenseeFilterSelections = _userService.GetLicenseeFilterSelections(CurrentUser.UserId);
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var licensees = _brandQueries.GetFilteredLicensees(allLicensees, CurrentUser.UserId)
                .Where(l => licenseeFilterSelections.Contains(l.Id))
                .OrderBy(l => l.Name)
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    Brands = _brandQueries.GetFilteredBrands(l.Brands, CurrentUser.UserId)
                        .Where(b =>
                            b.Status == BrandStatus.Active &&
                            brandFilterSelections.Contains(b.Id))
                        .OrderBy(b => b.Name)
                        .Select(b => new
                        {
                            b.Id,
                            b.Name,
                            VipLevels = b.VipLevels.Select(v => new { v.Code, v.Name }),
                            Currencies = b.BrandCurrencies.Select(v => v.Currency),
                            WalletTemplates = b.WalletTemplates.Select(wt => new { wt.Id, wt.Name, wt.IsMain }),
                            Products = b.Products.Select(p => p.ProductId)
                        })
                }).ToList();

            var bonuses =
                _bonusQueries.GetCurrentVersionBonuses()
                    .Select(bonus => new { bonus.Id, bonus.Name })
                    .OrderBy(bonus => bonus.Name);

            var products = _gameQueries.GetGameProviders().ToList();
            var games =
                _gameQueries.GetGameProviderConfigurations()
                    .Select(g => new { g.Id, g.Name, ProductId = g.GameProviderId, ProductName = products.Single(p => p.Id == g.GameProviderId).Name })
                    .ToList();                    

            if (templateVm != null && templateVm.Wagering != null)
            {
                foreach (var gc in templateVm.Wagering.GameContributions)
                {
                    var game = games.Single(g => g.Id == gc.GameId);
                    gc.Name = game.Name;
                    gc.ProductId = game.ProductId;
                    gc.ProductName = game.ProductName;
                }
            }
            
            var riskLevels = templateVm != null ? _riskLevelQueries.GetByBrand(templateVm.Info.BrandId.Value)
                .Select (riskLevel => new {riskLevel.Id, riskLevel.Name})
                .OrderBy(riskLevel => riskLevel.Name) : null;

            return Json(new
            {
                template = templateVm,
                notificationTemplates,
                bonuses,
                licensees,
                games,
                riskLevels
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateEdit(TemplateVM data)
        {
            var buildResult = _bonusBuilder.BuildTemplate(data);
            if (buildResult.IsValid)
            {
                var template = buildResult.Entity;
                _bonusCommands.AddUpdateTemplate(template);
                return Json(new { Success = true, template.Id, template.Version });
            }

            return Json(new { Success = false, buildResult.Errors });
        }

        [HttpPost]
        public ActionResult Delete(Guid templateId)
        {
            var validationResult = _bonusQueries.GetValidationResult(templateId);
            if (validationResult.IsValid == false)
                return Json(new { Success = false, validationResult.Errors });

            _bonusCommands.DeleteTemplate(templateId);
            return Json(new { Success = true });
        }

        private object MapTemplateCell(Template template, List<Bonus> bonuses)
        {
            var usedInBonuses = bonuses.Any(a => a.Template.Id == template.Id && a.IsActive);

            return new object[]
            {
                template.Info.Name,
                template.Info.Brand.LicenseeName,
                template.Info.Brand.Name,
                template.Info.Mode.ToString(),
                BonusQueries.GetTemplateType(template.Info),
                template.Status,
                template.CreatedBy,
                Format.FormatDate(template.CreatedOn),
                template.UpdatedBy,
                Format.FormatDate(template.UpdatedOn),
                usedInBonuses
            };
        }
    }
}