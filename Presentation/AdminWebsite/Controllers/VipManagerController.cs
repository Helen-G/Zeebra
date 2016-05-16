using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class VipManagerController : BaseController
    {
        private readonly BrandQueries _brandQueries;
        private readonly IGameQueries _gameQueries;
        private readonly BrandCommands _brandCommands;
        private readonly UserService _userService;
        private readonly PlayerCommands _playerCommands;
        private readonly IPlayerQueries _playerQueries;

        public VipManagerController(
            BrandQueries brandQueries,
            BrandCommands brandCommands,
            IGameQueries gameQueries,
            IPlayerQueries playerQueries,
            UserService userService,
            PlayerCommands playerCommands)
        {
            _brandQueries = brandQueries;
            _brandCommands = brandCommands;
            _gameQueries = gameQueries;
            _userService = userService;
            _playerCommands = playerCommands;
            _playerQueries = playerQueries;
        }

        // TODO Permissions
        [SearchPackageFilter("searchPackage")]
        public object List(SearchPackage searchPackage)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(CurrentUser.UserId);

            var vipLevels = _brandQueries.GetFilteredVipLevels(_brandQueries.GetVipLevels(), CurrentUser.UserId)
                .Where(x => brandFilterSelections.Contains(x.Brand.Id));

            var dataBuilder = new SearchPackageDataBuilder<VipLevel>(searchPackage, vipLevels);

            dataBuilder.Map(v => v.Id, v => new object[]
            {
                v.Name,
                v.Code,
                v.Brand.Name,
                v.Rank.ToString(),
                v.Brand.DefaultVipLevelId != null && v.Brand.DefaultVipLevelId == v.Id ? "Yes" : "No",
                v.Status.ToString(),
                v.CreatedBy,
                Format.FormatDate(v.DateCreated, false),
                v.UpdatedBy,
                Format.FormatDate(v.DateUpdated, false)/*,
                _playerCommands.GetPlayersCountWithVipLevel(v.Id) > 0*/
            })
            .DoOrder(x => x.Rank);

            var data = dataBuilder.GetPageData(v => v.Id);
            return new JsonResult { Data = data, MaxJsonLength = int.MaxValue, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public object GetCurrencies(Guid brandId)
        {
            return SerializeJson(new
            {
                Currencies = _brandQueries.GetCurrenciesByBrand(brandId).OrderBy(x => x.Code).Select(x => new { x.Code }),
            });
        }

        public object GetGames()
        {
            return SerializeJson(new
            {
                Games = _gameQueries.GetGameDtos().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name })
            });
        }

        public object DoPlayersExistOnVipLevel(Guid vipLevelId)
        {
            return SerializeJson(new
            {
                Result = _playerQueries.GetPlayersByVipLevel(vipLevelId).Any()
            });
        }

        public object GetDataForDeactivation(Guid vipLevelId)
        {
            var currentVipLevel = _brandQueries.GetVipLevel(vipLevelId);

            var newVipLevels = _brandQueries.GetVipLevels()
                .Where(o => o.Id != vipLevelId
                            && o.Brand.DefaultVipLevelId != o.Id
                            && o.Brand.Id == currentVipLevel.Brand.Id
                            && o.Status == VipLevelStatus.Active)
                .ToList();

            return SerializeJson(new
            {
                OldVipLevel = currentVipLevel.Name,
                NewVipLevels = newVipLevels.Select(o => new
                {
                    o.Id,
                    o.Name
                })
            });
        }

        [HttpPost]
        public string DeactivateVipLevel(Guid id, string remark, Guid? newVipLevelId)
        {
            _brandCommands.DeactivateVipLevel(id, remark, newVipLevelId);
            if (newVipLevelId != null)
                _playerCommands.ChangePlayerVipLevel(id, newVipLevelId);

            return SerializeJson(new { Result = "success" });
        }

        [HttpPost]
        public string ActivateVipLevel(Guid id, string remark)
        {
            try
            {
                _brandCommands.ActivateVipLevel(id, remark);
            }
            catch (RegoException ex)
            {
                return SerializeJson(new { Result = "failed", Message = ex.Message });
            }

            return SerializeJson(new { Result = "success" });
        }

        [HttpPost]
        public ActionResult Add(VipLevelViewModel model)
        {

            var validationResult = _brandCommands.ValidateThatVipLevelCanBeAdded(model);
            if (!validationResult.IsValid)
                return ValidationErrorResponseActionResult(validationResult.Errors);

            var id = _brandCommands.AddVipLevel(model);

            return this.Success(new { id });
        }

        [HttpPost]
        public ActionResult Edit(VipLevelViewModel model)
        {
            try
            {
                _brandCommands.EditVipLevel(model);

                return this.Success("app:vipLevel.edited");
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponseActionResult(e);
            }
        }

        [HttpGet]
        public string VipLevel(Guid id)
        {
            var vipLevel = _brandQueries.GetVipLevelViewModel(id);
            return SerializeJson(new
            {
                VipLevel = vipLevel
            });
        }

        [HttpGet]
        public string VipLevelView(Guid id)
        {
            var vipLevel = _brandQueries.GetVipLevel(id);
            var gameProviders = _gameQueries.GetGameProviders(vipLevel.Brand.Id);

            var data = new
            {
                licensee = vipLevel.Brand.Licensee.Name,
                brand = vipLevel.Brand.Name,
                defaultForNewPlayers = vipLevel.Brand.DefaultVipLevelId == vipLevel.Id,
                code = vipLevel.Code,
                name = vipLevel.Name,
                rank = vipLevel.Rank,
                description = vipLevel.Description,
                color = vipLevel.ColorCode,
                remark = vipLevel.UpdatedRemark,
                limits = vipLevel.VipLevelLimits.Select(x => new
                {
                    currency = x.Currency.Code,
                    gameProvider = gameProviders.Single(y => y.Id == x.GameProviderId).Name,
                    betLimit = _gameQueries.GetBetLimitDto(x.BetLimitId).Name
                })
            };

            return SerializeJson(data);
        }

        public string BetLimitData(Guid? brandId)
        {
            if (!brandId.HasValue)
                return SerializeJson(new
                {
                    Currencies = new object[] { },
                    GameProviders = new object[] { }
                });

            var data = new
            {
                Currencies = _brandQueries.GetCurrenciesByBrand(brandId.Value).OrderBy(x => x.Code).Select(x => new { x.Code }),
                GameProviders = _gameQueries.GetGameProviders(brandId.Value).Where(x => _gameQueries.GetBetLimits(x.Id, brandId.Value).Any())
            };

            return SerializeJson(data);
        }
    }
}