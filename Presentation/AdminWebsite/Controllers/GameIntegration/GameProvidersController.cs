using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using AFT.RegoV2.AdminWebsite.Common;
using AFT.RegoV2.AdminWebsite.Common.jqGrid;
using AFT.RegoV2.AdminWebsite.ViewModels.GameIntegration;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AutoMapper;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class GameProvidersController : BaseController
    {
        private readonly IGameQueries _queries;
        private readonly IGameManagement _gameManagement;
        private readonly RandomNumberGenerator randNumGen;
        private const int SecurityKeySize = 32;
        private const int AuthorizationSecretSize = 32;

        public GameProvidersController(
            IGameManagement gameManagement,
            IGameQueries queries)
        {
            _queries = queries;
            _gameManagement = gameManagement;
            randNumGen = new RNGCryptoServiceProvider();
        }

        [SearchPackageFilter("searchPackage")]
        public ActionResult Data(SearchPackage searchPackage)
        {
            var query = _queries.GetGameProviderDtos();
            var dataBuilder = new SearchPackageDataBuilder<GameProviderDTO>(searchPackage, query.AsQueryable());

            dataBuilder
                .Map(gameProvider => gameProvider.Id,
                    gameProvider => new[]
                    {
                        gameProvider.Name,
                        gameProvider.Category.ToString(),
                        gameProvider.IsActive ? "Active" : "Inactive",
                        gameProvider.CreatedBy,
                        Format.FormatDate(gameProvider.CreatedDate, false),
                        gameProvider.UpdatedBy,
                        Format.FormatDate(gameProvider.UpdatedDate, false),
                    }
                );
            var data = dataBuilder.GetPageData(gameProvider => gameProvider.Id);

            return new JsonResult
            {
                Data = data,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public string GetById(Guid id)
        {
            var gameProvider = _queries.GetGameProviderDtos().SingleOrDefault(gs => gs.Id == id);
            return gameProvider == null ? null : SerializeJson(gameProvider);
        }

        [HttpGet]
        public string GetEditData()
        {
            var categoryEnumType = typeof (GameProviderCategory);
            var categories =
                Enum.GetValues(categoryEnumType)
                    .Cast<int>()
                    .Select(x => new {Value = x, Name = Enum.GetName(categoryEnumType, x)});

            var authEnumType = typeof (AuthenticationMethod);
            var authenticationItems =
                Enum.GetValues(authEnumType)
                    .Cast<int>()
                    .Select(x => new {Value = x, Name = Enum.GetName(authEnumType, x)});

            var response = new
            {
                Categories = categories,
                AuthenticationItems = authenticationItems
            };
            return SerializeJson(response);
        }

        // TODO validation
        [HttpPost]
        public string Update(EditGameProviderModel model)
        {
            GameProvider gameProvider = null;
            if (model.Id.HasValue)
            {
                gameProvider = _queries.GetGameProviders().SingleOrDefault(x => x.Id == model.Id);
                if (gameProvider == null)
                {
                    return SerializeJson(new {Result = "failed", Message = "app:common.idDoesNotExist"});
                }
            }

            if (_queries.GetGameProviderDtos().Any(x => x.Name == model.Name && (!model.Id.HasValue || x.Id != model.Id.Value)))
                ModelState.AddModelError("Name", "{\"text\": \"app:common.nameUnique\"}");


            DateTimeOffset? time = null;
            if (model.SecurityKeyExpiryDate != null)
            {
                // TODO Currently expecting a date from client and assuming server timezone.
                time = DateTimeOffset.Parse(model.SecurityKeyExpiryDate);
                if ((gameProvider == null || gameProvider.SecurityKeyExpiryTime != time) && time < DateTimeOffset.Now)
                {
                    ModelState.AddModelError("SecurityKeyExpiryDate", "{\"text\": \"app:common.expiryDateInPast\"}");
                }
            }

            if (!ModelState.IsValid)
            {
                var fields = ModelState.Where(p => p.Value.Errors.Count > 0)
                    .Select(x => new {Name = ToCamelCase(x.Key), Errors = x.Value.Errors.Select(e => e.ErrorMessage)});
                return SerializeJson(new {Result = "failed", Fields = fields});
            }

            if (gameProvider == null)
            {
                gameProvider = Mapper.Map<GameProvider>(model);
            }
            else
            {
                Mapper.Map(model, gameProvider);
            }
            gameProvider.SecurityKeyExpiryTime = time;

            string message;
            if (model.Id.HasValue)
            {
                _gameManagement.UpdateGameProvider(gameProvider);
                message = "app:gameIntegration.gameProviders.gameProviderUpdated";
            }
            else
            {
                _gameManagement.CreateGameProvider(gameProvider);
                message = "app:gameIntegration.gameProviders.gameProviderCreated";
            }

            return SerializeJson(new {Result = "success", Data = message});
        }

        //TODO Evaluate if we need to impose permissions on these methods.
        public string GenerateAuthorizationClientId()
        {
            return SerializeJson(new {Result = "success", Data = Guid.NewGuid().ToString()});
        }

        public string GenerateAuthorizationSecret()
        {
            var bytes = new byte[AuthorizationSecretSize];
            randNumGen.GetBytes(bytes);
            return SerializeJson(new {Result = "success", Data = EncodeBase64Url(bytes)});
        }

        public string GenerateSecurityKey()
        {
            var bytes = new byte[SecurityKeySize];
            randNumGen.GetBytes(bytes);
            return SerializeJson(new {Result = "success", Data = EncodeBase64Url(bytes)});
        }

        private static string EncodeBase64Url(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace("=", string.Empty).Replace('+', '-').Replace('/', '_');
        }
    }
}