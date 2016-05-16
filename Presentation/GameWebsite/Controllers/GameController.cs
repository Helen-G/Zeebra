using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameWebsite.Helpers;
using AFT.RegoV2.GameWebsite.Models;
using DotNetOpenAuth.OAuth2;
using ServiceStack.ServiceClient.Web;

namespace AFT.RegoV2.GameWebsite.Controllers
{
    public class GameController : Controller
    {
        private bool IsOAuth 
        {
            get { return Session["IsOAuth"] != null && Convert.ToBoolean(Session["IsOAuth"]);}
            set { Session["IsOAuth"] = value; }
        }
        private IAuthorizationState AuthState
        {
            get { return Session["AuthState"] as IAuthorizationState; }
            set { Session["AuthState"] = value; }
        }

        private string _tokenEndpoint
        {
            get { return new AppSettings().GameApiUrl + "api/oauth/token"; }
        }

        private string GameName
        {
            get { return Session["currentGameName"] as string; }
            set { Session["currentGameName"] = value; }
        }

        private string OAuth2ClientId { get { return "MOCK_CLIENT_ID"; } }
        private string OAuth2ClientSecret { get { return "MOCK_CLIENT_SECRET"; } }

        private List<GameProviderBetLimit> _betLimits = new List<GameProviderBetLimit>
        {
            new GameProviderBetLimit {BetLimitId = "10", CurrencyCode = "CAD", MinValue = 10, MaxValue = 1000},
            new GameProviderBetLimit {BetLimitId = "20", CurrencyCode = "CAD", MinValue = 20, MaxValue = 2000},
            new GameProviderBetLimit {BetLimitId = "30", CurrencyCode = "CAD", MinValue = 30, MaxValue = 3000},
            new GameProviderBetLimit {BetLimitId = "10", CurrencyCode = "CNY", MinValue = 10, MaxValue = 1000},
            new GameProviderBetLimit {BetLimitId = "20", CurrencyCode = "CNY", MinValue = 20, MaxValue = 2000},
            new GameProviderBetLimit {BetLimitId = "30", CurrencyCode = "CNY", MinValue = 30, MaxValue = 3000}
        };



        public ActionResult Index(string token, bool? isOAuth=null, string gameName = null)
        {
            if (Session["IsOAuth"] == null && isOAuth.HasValue)
                IsOAuth = isOAuth.Value;

            if (token == null)
            {
                return View(new GameViewModel {Message = "Welcome to the mock Game Server!"});
            }
            var response = ValidateToken(token);

            if (gameName != null)
                GameName = gameName;

            return
                View(new GameViewModel
                {
                    GameName = GameName ?? "",
                    Token = token,
                    Rounds = GetRounds(token),
                    Enabled = true,
                    Balance = response.Balance,
                    PlayerName = response.PlayerDisplayName,
                    BrandCode = response.BrandCode,
                    CurrencyCode = response.CurrencyCode,
                    Language = response.Language,
                    BetLimitCode = response.BetLimitCode
                });
        }

        private ValidateTokenResponse ValidateToken(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
                throw new WebServiceException("Access denied (missing token)");

            EnsureAccessToken();

            var response = GameApiUtil.CallGameApiPost<ValidateToken, ValidateTokenResponse>(
                            "token/validate",
                            new ValidateToken()
                            {
                                AuthToken = token,
                                PlayerIpAddress = Request.UserHostAddress
                            }, OAuthHeaderDecoration);

            if ( response.ErrorCode != 0)
                throw new WebServiceException(string.Format("Access denied (invalid token), Error Code: {0}, description: {1}",
                                response.ErrorCode, response.ErrorDescription));

            return response;
        }

        
        #region decorate request (for OAuth)
        private void EnsureAccessToken()
        {
            if (IsOAuth)
                PrepareAccessToken();
        }

        private void PrepareAccessToken()
        {
            if (AuthState != null && AuthState.AccessTokenExpirationUtc > DateTimeOffset.UtcNow.AddSeconds(1))
                return;

            var serverDescription = new AuthorizationServerDescription
            {
                TokenEndpoint = new Uri(_tokenEndpoint)
            };

            var client = new WebServerClient(serverDescription, OAuth2ClientId, OAuth2ClientSecret);

            if (AuthState == null || AuthState.RefreshToken == null)
            {
                var scopes = "bets players transactions".Split(' ');
                AuthState = client.GetClientAccessToken(scopes);
            }
            else
            {
                client.RefreshAuthorization(AuthState);
            }
        }
        #endregion


        public ActionResult PlaceInitialBet(GameViewModel model)
        {
            EnsureAccessToken();

            var txList = new List<PlaceBetTransaction>
            {
                new PlaceBetTransaction
                {
                    Amount = model.Amount,
                    CurrencyCode = "CAD",
                    RoundId = Guid.NewGuid().ToString(),
                    Description = model.Description,
                    GameTypeId = "Mock",
                    Id = Guid.NewGuid().ToString(),
                    TimeStamp = DateTimeOffset.UtcNow
                }
            };

            var response = CallPlaceBet(model.Token, model.BetLimitCode, txList);

            if (response.ErrorCode != GameApiErrorCode.NoError)
                throw new GameApiException(response.ErrorDescription);

            return RedirectToAction("Index", new {token = model.Token});
        }

        private Action<WebHeaderCollection> OAuthHeaderDecoration
        {
            get
            {
                return IsOAuth 
                        ? headers => ClientBase.AuthorizeRequest(headers, AuthState.AccessToken)
                        : (Action<WebHeaderCollection>)null;
            }
        }

        #region Call GameApi methods

        private PlaceBetResponse CallPlaceBet(string token, string betLimitId, List<PlaceBetTransaction> txList)
        {
            ValidateBetLimits(betLimitId, txList);

            return GameApiUtil.CallGameApiPost<PlaceBet, PlaceBetResponse>(
                            "bets/place",
                            new PlaceBet
                            {
                                AuthToken = token,
                                Transactions = txList
                            }, OAuthHeaderDecoration);

        }

        private void ValidateBetLimits(string betLimitId, IEnumerable<PlaceBetTransaction> txList)
        {
            var betLimitsValidator = new GameProviderBetLimitValidator(_betLimits);

            foreach (var placeBetTransaction in txList)
            {
                betLimitsValidator.Validate(betLimitId, placeBetTransaction.CurrencyCode, placeBetTransaction.Amount);    
            }
        }


        private WinBetResponse CallWinBet(string token, List<SettleBetTransaction> txList)
        {
            return GameApiUtil.CallGameApiPost<WinBet, WinBetResponse>(
                            "bets/win",
                            new WinBet
                        {
                            AuthToken = token,
                            Transactions = txList
                        }, OAuthHeaderDecoration);
        }


        private LoseBetResponse CallLoseBet(string token, List<SettleBetTransaction> txList)
        {
            return GameApiUtil.CallGameApiPost<LoseBet, LoseBetResponse>(
                            "bets/lose",
                            new LoseBet
                            {
                                AuthToken = token,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }


        private FreeBetResponse CallFreeBet(string token, string betLimitId, List<SettleBetTransaction> txList)
        {
            ValidateBetLimits(betLimitId, txList.Select(x => new PlaceBetTransaction
            {
                Amount = x.Amount,
                CurrencyCode = x.CurrencyCode
            }));

            return GameApiUtil.CallGameApiPost<FreeBet, FreeBetResponse>(
                            "bets/freebet",
                            new FreeBet
                            {
                                AuthToken = token,
                                Transactions = txList
                            }, OAuthHeaderDecoration);

        }

        private AdjustTransactionResponse CallAdjustTransaction(string token, List<AdjustTransactionData> txList)
        {
            return GameApiUtil.CallGameApiPost<AdjustTransaction, AdjustTransactionResponse>(
                            "transactions/adjust",
                            new AdjustTransaction
                            {
                                AuthToken = token,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }

        private CancelTransactionResponse CallCancelTransaction(string token, List<CancelTransactionData> txList)
        {
            return GameApiUtil.CallGameApiPost<CancelTransaction, CancelTransactionResponse>(
                            "transactions/cancel",
                            new CancelTransaction
                            {
                                AuthToken = token,
                                Transactions = txList
                            }, OAuthHeaderDecoration);
        }
        #endregion

        public ActionResult PlaceTonsOfBets(GameViewModel model)
        {
            return RedirectToAction("Index", new { token = model.Token });
            
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Place")]
        public ActionResult PlaceBet(GameViewModel model)
        {

            try
            {
                EnsureAccessToken();

                var txList = new List<PlaceBetTransaction>
                {
                    new PlaceBetTransaction
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        GameTypeId = "Mock",
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallPlaceBet(model.Token, model.BetLimitCode, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameProviderBetLimitException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }
            catch (WebServiceException ex)
            {
                return View("Index", new GameViewModel { Message = ex.ResponseBody });
            }
        }

    

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Win")]
        public ActionResult WinBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<SettleBetTransaction>
                {
                    new SettleBetTransaction
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallWinBet(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }
        
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Close")]
        public ActionResult CloseBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<SettleBetTransaction>
                {
                    new SettleBetTransaction
                    {
                        Amount = 0,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow
                    }
                };

                var response = CallLoseBet(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                


                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Cancel")]
        public ActionResult CancelBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<CancelTransactionData>
                {
                    new CancelTransactionData
                    {
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                        ReferenceId = model.TransactionId
                    }
                };

                var response = CallCancelTransaction(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Adjust")]
        public ActionResult AdjustBet(GameViewModel model)
        {
            try
            {
                EnsureAccessToken();

                var txList = new List<AdjustTransactionData>
                {
                    new AdjustTransactionData
                    {
                        Amount = model.OperationAmount,
                        CurrencyCode = "CAD",
                        RoundId = model.RoundId.ToString(),
                        Description = model.Description,
                        Id = Guid.NewGuid().ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                        ReferenceId = model.TransactionId
                    }
                };

                var response = CallAdjustTransaction(model.Token, txList);

                if (response.ErrorCode != GameApiErrorCode.NoError)
                {
                    throw new GameApiException(response.ErrorDescription);
                }                

                return RedirectToAction("Index", new { token = model.Token });
            }
            catch (GameApiException ex)
            {
                return View("Index", new GameViewModel { Message = ex.Message });
            }

        }
        

        List<RoundHistoryData> GetRounds(string token)
        {
            EnsureAccessToken();

            var roundsHistoryResponse = 
                GameApiUtil.CallGameApiGet<BetsHistoryResponse>("bets/history?authtoken="+ token, OAuthHeaderDecoration);
            return roundsHistoryResponse.Rounds.OrderByDescending(a => a.CreatedOn).ToList();
        }
    }

    
}