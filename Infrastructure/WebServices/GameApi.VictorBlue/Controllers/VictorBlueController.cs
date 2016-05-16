using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.ServiceContracts.VictorBlue;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.GameApi.VictorBlue.Attributes;
using AFT.RegoV2.Infrastructure.Attributes;
using AFT.RegoV2.Infrastructure.Providers;

namespace AFT.RegoV2.GameApi.VictorBlue.Controllers
{
    [ForceJsonFormatter(additionalMediaTypes:"application/x-www-form-urlencoded")]
    public class VictorBlueController : ApiController
    {
        private readonly IGameProviderLog _log;
        private readonly IGamesCommonOperationsProvider _commonOperations;
        private readonly ITokenProvider _tokenProvider;
        private readonly ITokenValidationProvider _tokenValidation;

        private const string CurrencyCode = "CNY";

        #region Static Service Handler Map
        // we need to have the map static so we don't recreate it on each request
        private static readonly IDictionary<string, Func<VictorBlueController, Func<CommonMessage, Task<ResponseBase>>>> ServiceMap = GetServiceMap();
        
        private static IDictionary<string, Func<VictorBlueController, Func<CommonMessage, Task<ResponseBase>>>> GetServiceMap()
        {
            return new Dictionary<string, Func<VictorBlueController, Func<CommonMessage, Task<ResponseBase>>>>(StringComparer.OrdinalIgnoreCase)
                {
                    {"PlayerLoginRequest", vbs => vbs.Login},
                    {"GetSingleWalletPlayerBalanceRequest", vbs => vbs.GetSingleWalletPlayerBalance},
                    {"UpdateSingleWalletPlayerBalanceRequest", vbs => vbs.UpdateSingleWalletPlayerBalance}
                };
        } 
        #endregion

        public VictorBlueController(
            IGameProviderLog log, 
            IGamesCommonOperationsProvider commonOperations,
            ITokenProvider tokenProvider,
            ITokenValidationProvider tokenValidation)
        {
            _log = log;
            _commonOperations = commonOperations;
            _tokenProvider = tokenProvider;
            _tokenValidation = tokenValidation;
        }

        [Route("api/victorblue"), ProcessVictorBlueError]
        public async Task<ResponseBase> Post(CommonMessage message)
        {
            // this log can be removed if stable
            _log.LogInfo(_log.RequestAsString(Request));
            
            Func<VictorBlueController, Func<CommonMessage, Task<ResponseBase>>> handler;
            if (ServiceMap.TryGetValue(message.Msg, out handler))
            {
                return await handler(this)(message);
            }
            throw new InvalidOperationException(String.Format("Unknown message '{0}'", message.Msg));
        }

        private TokenData GetTokenData<TRequest>(string token, string playerIp)
        {
            var tokenData = _tokenProvider.Decrypt(token);
            _tokenValidation.ValidateToken(tokenData, playerIp, typeof(TRequest));
            return tokenData;
        }

        private async Task<ResponseBase> Login(ILogin message)
        {
            var response = new LoginResponse
            {
                Transaction = message.Transaction,
                Result = (int)VictorBlueCodes.Success
            };
            var tokenData = GetTokenData<ValidateToken>(message.LoginToken, message.PlayerIp);

            await _commonOperations.ValidateTokenAsync(new ValidateToken
            {
                AuthToken = message.LoginToken,
                PlayerIpAddress = message.PlayerIp
            });

            return response;
        }
        private async Task<ResponseBase> GetSingleWalletPlayerBalance(IGetSingleWalletPlayerBalance message)
        {
            var tokenData = GetTokenData<ValidateToken>(message.RequestToken, message.PlayerIp);
            var balance = await _commonOperations.GetBalanceAsync(new GetBalance
            {
                AuthToken = message.RequestToken,
            });

            return new GetSingleWalletPlayerBalanceResponse
            {
                Balance = balance,
                Result = (int)VictorBlueCodes.Success
            };
        }
        private async Task<ResponseBase> UpdateSingleWalletPlayerBalance(IUpdateSingleWalletPlayerBalance message)
        {
            // their transaction ID is GUID-1-a for place bet and GUID-1-b for win bet, we extract the bet ID
            string roundId;
            if (!TryExtractRoundId(message.Transaction, out roundId))
            {
                roundId = message.Transaction;
            }

            var tokenData = GetTokenData<ValidateToken>(message.RequestToken, message.PlayerIp);
            decimal balance;
            if (message.Amount < 0)
            {
                var bet = new PlaceBet
                {
                    AuthToken = message.RequestToken,
                    Transactions = new List<PlaceBetTransaction>
                    {
                        new PlaceBetTransaction
                        {
                            RoundId = roundId,
                            Amount = message.Amount * -1,
                            CurrencyCode = CurrencyCode,
                            Id = message.Transaction,
                            TimeStamp = DateTimeOffset.UtcNow
                        }
                    }
                };

                var response = await _commonOperations.PlaceBet(bet);
                balance = response.Balance;
            }
            else if (message.Amount > 0)
            {
                var bet = new WinBet
                {
                    AuthToken = message.RequestToken, 
                    Transactions = new List<SettleBetTransaction>
                    {
                        new SettleBetTransaction
                        {
                            RoundId = roundId,
                            Amount = message.Amount,
                            CurrencyCode = CurrencyCode,
                            Id = message.Transaction,
                            TimeStamp = DateTimeOffset.UtcNow
                        }
                    }
                };

                var response = _commonOperations.WinBet(bet);
                balance = response.Balance;
            }
            else
            {
                throw new InvalidAmountException("Update wallet with amount 0 is not allowed");
            }
            return await Task.FromResult(new UpdateSingleWalletPlayerBalanceResponse
                            {
                                Transaction = message.Transaction,
                                RefId = message.RefId,
                                Balance = balance,
                                Result = (int) VictorBlueCodes.Success
                            });
        }

        internal static bool TryExtractRoundId(string transactionId, out string extracted)
        {
            if (transactionId != null && transactionId.Length > 36)
            {
                extracted = transactionId.Substring(0, 36);//extract guid
                return true;
            }
            extracted = transactionId;
            return false;
        }
    }

    public enum VictorBlueCodes
    {
        Unknown = -1,
        Success = 0,
        IpNotAllowed = 4,
        InvalidTransaction = 5,
        PlayerDisabled = 100,
        PlayerNotExist = 102,
        MerchantDisabled = 200,
        MerchantNotExist = 203,
        InvalidLoginToken = 300,
        LoginAuthenticationFailed = 302,
        InsufficientBalance = 401,
        InvalidRequestToken = 501,
    }
}