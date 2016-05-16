using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameWebsite.Helpers;
using AFT.RegoV2.GameWebsite.Models;
using DotNetOpenAuth.OAuth2;
using ServiceStack.ServiceClient.Web;

namespace AFT.RegoV2.GameWebsite.Controllers
{
    public class SecureGameController : Controller
    {
        private IAuthorizationState AuthState
        {
            get
            {
                return Session["SecureGameAuthState"] as IAuthorizationState;
            }
            set
            {
                Session["SecureGameAuthState"] = value;
            }
        }
        private string TokenEndpoint
        {
            get
            {
                return new AppSettings().GameApiUrl + "api/oauth/token";
            }
        }
        private string OAuth2ClientId { get { return "MOCK_CLIENT_ID"; } }
        private string OAuth2ClientSecret { get { return "MOCK_CLIENT_SECRET"; } }

        public ActionResult Index(string token)
        {
            try 
            {
                if (String.IsNullOrWhiteSpace(token))
                {
                    return View(new GameViewModel { Message = "Access denied (missing token)" });
                }

                PrepareAccessToken();

                var response = ValidateToken(token);

                if (response.ErrorCode != 0)
                {
                    return View(new GameViewModel
                    {
                        Message =
                            string.Format("Access denied (invalid token), Error Code: {0}, description: {1}", response.ErrorCode, response.ErrorDescription)
                    });
                }

                return View(new GameViewModel
                {
                    Token = token, Rounds = GetBets(token), Enabled = true, Balance = response.Balance, PlayerName = response.PlayerDisplayName,
                    AccessToken = AuthState.AccessToken,
                    AccessTokenExpiration = !AuthState.AccessTokenExpirationUtc.HasValue ? DateTime.MaxValue : AuthState.AccessTokenExpirationUtc.Value.ToLocalTime()
                });
            }
            catch (WebServiceException ex)
            {
                return View(new GameViewModel { Message = ex.ResponseBody });
            }

        }

        private void PrepareAccessToken()
        {
            if (AuthState != null && AuthState.AccessTokenExpirationUtc > DateTimeOffset.UtcNow.AddSeconds(1))
            {
                return;
            }

            var serverDescription = new AuthorizationServerDescription
            {
                TokenEndpoint = new Uri(TokenEndpoint)
            };

            var client = new WebServerClient(serverDescription, OAuth2ClientId, OAuth2ClientSecret);
            
            var scopes = "bets players transactions".Split(' ');

            AuthState = client.GetClientAccessToken(scopes);
        }

        private Action<WebHeaderCollection> OAuthHeaderDecoration { get { return headers => ClientBase.AuthorizeRequest(headers, AuthState.AccessToken); } }

        private ValidateTokenResponse ValidateToken(string token)
        {
            return GameApiUtil.CallGameApiPost<ValidateToken, ValidateTokenResponse>(
                            "token/validate",
                            new ValidateToken()
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress
                            }, OAuthHeaderDecoration);
        }


        List<RoundHistoryData> GetBets(string token)
        {
            var betsHistoryResponse = 
                GameApiUtil.CallGameApiGet<BetsHistoryResponse>("bets/history?authtoken="+ token, OAuthHeaderDecoration);

            return betsHistoryResponse.Rounds.OrderByDescending(a => a.CreatedOn).ToList();
        }

    }
}
