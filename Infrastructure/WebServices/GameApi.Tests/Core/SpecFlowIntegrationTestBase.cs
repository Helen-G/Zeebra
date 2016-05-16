using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AFT.RegoV2.Core.Game;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Infrastructure.DataAccess.Game;
using AFT.RegoV2.Infrastructure.Providers;
using AFT.RegoV2.MemberApi.Interface;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public abstract class SpecFlowIntegrationTestBase : SpecFlowUnitTestBase
    {
        protected const string DefaultPlayertIp = "50.67.208.24";
        protected const string DefaultBrand = "138";
        protected static readonly IJsonSerializationProvider Json = new JsonSerializationProvider();

        [StepArgumentTransformation(@"will (.*)be")]
        public bool WillNotBe(string word)
        {
            return word.Trim() != "not";
        }
        protected static string NewStringId { get { return Guid.NewGuid().ToString(); } }
        protected static IGameRepository GetOrCreateGamesDb()
        {
            return GetOrCreate<IGameRepository>(
                        SR.GsiDb, () => new GameRepository());
        }

        protected static Task<HttpResponseMessage> JsonHttp(HttpMethod method, string url, object content = null, IDictionary<string,string> headers = null )
        {
            var http = GetOrCreate(SR.http, () => new HttpClient());
            var req = new HttpRequestMessage(method, url);
            req.Headers.Remove(SR.HeaderAccept);
            req.Headers.Add(SR.HeaderAccept, "application/json");
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (content != null)
                {
                    req.Content = new StringContent(Json.SerializeToString(content));
                    req.Content.Headers.Remove(SR.HeaderContentType);
                    req.Content.Headers.Add(SR.HeaderContentType, "application/json");
                }
            }
            else if (content != null)
            {
                throw new InvalidOperationException("Unexpected to have content for method " + method);
            }
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    req.Headers.Remove(h.Key);
                    req.Headers.Add(h.Key, h.Value);
                }
            }
            return http.SendAsync(req);
        }

        protected static Task<T> JsonPost<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Post, url, content, headers);
        }
        protected static Task<T> JsonPut<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Put, url, content, headers);
        }
        protected static Task<T> JsonDelete<T>(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<T> JsonGet<T>(string url, IDictionary<string,string> headers = null)
        {
            return JsonHttp<T>(HttpMethod.Get, url, null, headers);
        }

        protected static Task<T> JsonPostSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Post, url, content, headers);
        }
        protected static Task<T> JsonPutSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Put, url, content, headers);
        }
        protected static Task<T> JsonDeleteSecure<T>(string url, string accessToken, object content = null, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<T> JsonGetSecure<T>(string url, string accessToken, IDictionary<string, string> headers = null)
        {
            SetAccessToken(accessToken, ref headers);
            return JsonHttp<T>(HttpMethod.Get, url, null, headers);
        }

        private static void SetAccessToken(string accessToken, ref IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            headers.Add(SR.HeaderAuthorization, "Bearer " + accessToken);
        }

        protected static Task<dynamic> JsonPost(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Post, url, content, headers);
        }
        protected static Task<dynamic> JsonPut(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Put, url, content, headers);
        }
        protected static Task<dynamic> JsonDelete(string url, object content = null, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Delete, url, content, headers);
        }
        protected static Task<dynamic> JsonGet(string url, IDictionary<string,string> headers = null)
        {
            return JsonHttpDynamic(HttpMethod.Get, url, null, headers);
        }

        protected static IDictionary<string, string> Headers(params string[] args)
        {
            return args == null || args.Length == 0 
                    ? null 
                    : args
                        .Select(a => a.Split(new[] {':'}, 2))
                        .ToDictionary(arr => arr[0].Trim(), arr => arr.Length < 2 ? "" : arr[1].Trim());
        }

        private static async Task<dynamic> JsonHttpDynamic(HttpMethod method, string url, object content, IDictionary<string, string> headers = null)
        {
            var res = await JsonHttp(method, url, content, headers);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeAsDynamic(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 
        }
        private static async Task<T> JsonHttp<T>(HttpMethod method, string url, object content, IDictionary<string,string> headers = null)
        {
            var res = await JsonHttp(method, url, content, headers);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeFromString<T>(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 
        }

        protected static Task<HttpResponseMessage> FormHttp(HttpMethod method, string url, FormUrlEncodedContent content = null, IDictionary<string, string> headers = null)
        {
            var http = GetOrCreate(SR.http, () => new HttpClient());
            var req = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (content != null)
                {
                    req.Content = content;
                    req.Content.Headers.Remove(SR.HeaderContentType);
                    req.Content.Headers.Add(SR.HeaderContentType, "application/x-www-form-urlencoded");
                }
            }
            else if (content != null)
            {
                throw new InvalidOperationException("Unexpected to have content for method " + method);
            }
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    req.Headers.Remove(h.Key);
                    req.Headers.Add(h.Key, h.Value);
                }
            }
            return http.SendAsync(req);
        }

        private static async Task<dynamic> FormHttpDynamic(HttpMethod method, string url, FormUrlEncodedContent content, IDictionary<string, string> headers = null)
        {
            var res = await FormHttp(method, url, content, headers);
            var json = await res.Content.ReadAsStringAsync();
            try
            {
                return Json.DeserializeAsDynamic(json);
            }
            catch (JsonReaderException jre)
            {
                throw new InvalidDataException("Cannot parse JSON", jre);
            } 

        }

        protected static Task<dynamic> FormPost(string url, FormUrlEncodedContent content = null, IDictionary<string, string> headers = null)
        {
            return FormHttpDynamic(HttpMethod.Post, url, content, headers);
        }


        protected async Task<string> GetTokenFor(string player, string password, string game)
        {

            var tokenContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", player),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("BrandId", "00000000-0000-0000-0000-000000000138"),
                new KeyValuePair<string, string>("IpAddress", "::1"),
                new KeyValuePair<string, string>("BrowserHeaders", "")
            });

            dynamic tokenResult = await FormPost(Config.MemberApiUrl + "token", tokenContent);

            dynamic result = 
                await JsonPost(Config.MemberApiUrl + "api/player/login",
                      new
                      {
                          brandId = "00000000-0000-0000-0000-000000000138", 
                          username = player, 
                          password, 
                          cultureCode = "en-US",
                      });

            var gsiDb = GetOrCreateGamesDb();

            var token = JsonConvert.DeserializeObject<TokenResponse>(tokenResult.ToString()).AccessToken;

            var gInfo = gsiDb.Games.Where(g => g.Name == game).Select(g => new {g.Id, g.GameProviderId}).FirstOrDefault();

            if (gInfo == null)
            {
                throw new ArgumentException("Game "+game+" not found");
            }

            Set(SR.GameProviderId, gInfo.GameProviderId);

            result = 
                await JsonGet(
                        Config.MemberApiUrl + "api/games/gameredirect" +
                        "?gameId=" + gInfo.Id + 
                        "&gameProviderId=" + gInfo.GameProviderId + 
                        "&playerIpAddress="+HttpUtility.UrlEncode(DefaultPlayertIp)+
                        "&brandCode="+HttpUtility.UrlEncode(DefaultBrand), 
                        Headers(
                        SR.HeaderCookie+":CultureCode=en-US", 
                        SR.HeaderAuthorization + ":Bearer " + token));

            string url = result.url;
            var queries = HttpUtility.ParseQueryString(url.Split('?')[1]);
            return queries["LoginTokenID"] ?? queries["token"] ?? queries["authToken"];
        }

        protected async Task<string> GetAccessTokenFor(string clientId, string clientSecret)
        {
            dynamic result =
                await FormPost(Config.GameApiUrl + "api/soleil/oauth/token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        {"client_id", clientId},
                        {"client_secret", clientSecret},
                        {"grant_type", "client_credentials"},
                        {"scope", "bets"}
                    }));


            return result.access_token;
        }

        protected Uri BuildUri(string relativePath)
        {
            return new Uri(new Uri(Config.GameApiUrl), relativePath);
        }

        protected string GetFullApiPath(string relativePath)
        {
            return BuildUri(relativePath).ToString();
        }
    }
}