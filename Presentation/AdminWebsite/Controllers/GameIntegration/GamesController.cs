using System;
using System.Linq;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Game.Interfaces;
using ServiceStack.Validation;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class GamesController : BaseController
    {
        private readonly IGameQueries _gameQueries;
        private readonly IGameManagement _gameManagement;

        public GamesController(
            IGameQueries gameQueries,
            IGameManagement gameCommands)
        {
            _gameQueries = gameQueries;
            _gameManagement = gameCommands;
        }

        public string Products()
        {
            var products = _gameQueries.GetGameProviderDtos();

            return SerializeJson(products.Select(x => new {x.Id, x.Name}));
        }

        public string Types()
        {
            return SerializeJson(new []{"Casino", "Poker", "Sportbets"});
        }

        public string Statuses()
        {
            return SerializeJson(new[] { "Active", "Inactive" });
        }

        [HttpGet]
        public string Game(Guid id)
        {
            var game = _gameQueries.GetGameDto(id);
            return SerializeJson(game);
        }

        public string Delete(Guid id)
        {
            _gameManagement.DeleteGame(id);
            return SerializeJson(new {Success = true});
        }

        [HttpPost]
        public ActionResult Game(GameDTO viewModel)
        {
            try
            {
                var message = String.Empty;
                if (viewModel.Id != null)
                {
                    _gameManagement.UpdateGame(viewModel);
                    message = "app:gameIntegration.games.updated";
                }
                else
                {
                    _gameManagement.CreateGame(viewModel);
                    message = "app:gameIntegration.games.created";
                }
                    
                return this.Success(message);
            }
            catch (ValidationError e)
            {
                return ValidationErrorResponseActionResult(e);
            }
            catch (Exception e)
            {
                return this.Failed(e);
            }
        }

        #region List

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            return new JsonResult
            {
                Data = SearchData(searchPackage),
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        protected object SearchData(SearchPackage searchPackage)
        {
            var query = _gameQueries.GetGameDtos();
            var dataBuilder = new SearchPackageDataBuilder<GameDTO>(searchPackage, query.AsQueryable());
            var gameProviders = _gameQueries.GetGameProviderDtos().ToList();

            
            dataBuilder
                .Map(game => game.Id,
                    game => new[]
                    {
                        gameProviders.FirstOrDefault(x => x.Id == game.ProductId) == null
                            ? string.Empty
                            : gameProviders.FirstOrDefault(x => x.Id == game.ProductId).Name,
                        game.Name,
                        game.Type,
                        game.Status,
                        game.CreatedBy,
                        Format.FormatDate(game.CreatedDate, false),
                        game.UpdatedBy,
                        Format.FormatDate(game.UpdatedDate, false),

                    }
                );
            return dataBuilder.GetPageData(game => game.Id);
        }

        #endregion
    }
}