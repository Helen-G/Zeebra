using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.MemberApi.Interface.GameProvider;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class GamesController : BaseApiController
    {
        private readonly IGameQueries _gameQueries;
        private readonly ITokenProvider _tokenProvider;

        public GamesController(ITokenProvider tokenProvider, IGameQueries gameQueries)
        {
            _tokenProvider = tokenProvider;
            _gameQueries = gameQueries;
        }

        [HttpGet]
        public GameListResponse GameList([FromUri]GameListRequest request)
        {
            var brandId = _gameQueries.GetPlayerData(PlayerId).BrandId;

            var gps = _gameQueries.GetGameProvidersWithGames(brandId).ToList();
            return new GameListResponse
            {
                GameProviders = gps
                    .Select(gameProvider => new GameProviderData
                    {
                        Id = gameProvider.Id,
                        Name = gameProvider.Name,
                        Games = gameProvider.Games.Select(game => new GameData
                        {
                            Id = game.Id,
                            Name = game.Name
                        }).ToList()
                    }).ToList()
            };
        }

        [HttpGet]
        public async Task<GameRedirectResponse> GameRedirect([FromUri]GameRedirectRequest request)
        {
            const long unixEpoch = 621355968000000000L;

            var player = _gameQueries.GetPlayerData(PlayerId);

            var game = await _gameQueries.GetGameByIdAsync(request.GameId);
            
            var brand = _gameQueries.GetBrand(request.BrandCode);

            var token = _tokenProvider.Encrypt(new TokenData
            {
                TokenId = Guid.NewGuid(),
                Time = (DateTime.UtcNow.ToUniversalTime().Ticks - unixEpoch) / TimeSpan.TicksPerSecond,
                GameId = request.GameId,
                PlayerId = player.Id,
                BrandId = brand.Id,
                PlayerIpAddress = request.PlayerIpAddress,
                CurrencyCode = player.Currency.Code ?? string.Empty
            });

            var gameProviderConfiguration = await _gameQueries.GetGameProviderConfigurationAsync(brand.Id, game.GameProviderId);
            Uri gameUri;
            var url = gameProviderConfiguration.Endpoint + game.EndpointPath;

            bool isPostRequest;
            try
            {
                gameUri = new Uri(GenerateUrl(url, token, player, game.Name, gameProviderConfiguration.Code, out isPostRequest));
            }
            catch
            {
                throw new ApplicationException(String.Format("Invalid game provider configuration at {0}.", url));
            }

            return new GameRedirectResponse
            {
                Url = gameUri,
                IsPostRequest = isPostRequest
            };
        }

        const string TemplatePrefix = "template:";
        const string PostPrefix = "post:";
        const string GetPrefix = "get:";
        private string GenerateUrl(string url, string token, Player player, string gameName, string code, out bool isPostRequest)
        {
            isPostRequest = false;

            if (string.IsNullOrEmpty(url)) throw new ArgumentException();

            if (url.StartsWith(TemplatePrefix, StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(TemplatePrefix.Length);

                if (url.StartsWith(GetPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(GetPrefix.Length);
                }
                else if (url.StartsWith(PostPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    isPostRequest = true;
                    url = url.Substring(PostPrefix.Length);
                }
                url = url.Replace("{PlayerName}", player.Name);
                url = url.Replace("{Token}", token);
                url = url.Replace("{Lang}", player.CultureCode.Split('-')[0]);
                url = url.Replace("{Currency}", player.Currency.Code);
                return url;
            }

            url = url.Replace("{GameName}", gameName);
            url = url.Replace("{Code}", code);

            var limiter = url.IndexOf('?') > 0 ? '&' : '?';
            return url + limiter + "token=" + token;
        }
    }
}