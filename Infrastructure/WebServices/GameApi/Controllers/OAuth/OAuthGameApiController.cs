using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.GameApi.Interface.Attributes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.Attributes;

namespace AFT.RegoV2.GameApi.Controllers.OAuth
{
    [ForceJsonFormatter]
    [Attributes.WebApiRequireOAuth2Scope("bets")]
    public class OAuthGameApiController : ApiController
    {
        private readonly IGamesCommonOperationsProvider _common;

        public OAuthGameApiController(IGamesCommonOperationsProvider common)
        {
            _common = common;
        }

        [Route("oauth/token/validate"), ValidateTokenData, ProcessError]
        public ValidateTokenResponse Post(ValidateToken request)
        {
            return _common.ValidateToken(request);
        }
        [Route("oauth/players/balance"), ValidateTokenData, ProcessError]
        public GetBalanceResponse Get([FromUri]GetBalance request)
        {
            return _common.GetBalance(request);
        }
        [Route("oauth/bets/place"), ValidateTokenData, ProcessError]
        public async Task<PlaceBetResponse> Post(PlaceBet request)
        {
            return await _common.PlaceBet(request);
        }
        [Route("oauth/bets/win"), ValidateTokenData, ProcessError]
        public WinBetResponse Post(WinBet request)
        {
            return _common.WinBet(request);
        }
        [Route("oauth/bets/lose"), ValidateTokenData, ProcessError]
        public LoseBetResponse Post(LoseBet request)
        {
            return _common.LoseBet(request);
        }
        [Route("oauth/bets/freebet"), ValidateTokenData, ProcessError]
        public FreeBetResponse Post(FreeBet request)
        {
            return _common.FreeBet(request);
        }
        [Route("oauth/transactions/adjust"), ValidateTokenData, ProcessError]
        public AdjustTransactionResponse Post(AdjustTransaction request)
        {
            return _common.AdjustTransaction(request);
        }
        [Route("oauth/transactions/cancel"), ValidateTokenData, ProcessError]
        public CancelTransactionResponse Post(CancelTransaction request)
        {
            return _common.CancelTransaction(request);
        }
        [Route("oauth/batch/bets/settle"), ValidateTokenData, ProcessError]
        public async Task<SettleBetsResponse> Post(SettleBets request)
        {
            return await _common.SettleBets(request);
        }
        [Route("oauth/batch/transactions/adjust"), ProcessError]
        public AdjustTransactionsResponse Post(AdjustTransactions request)
        {
            return _common.AdjustTransactions(request);
        }
        [Route("oauth/batch/transactions/cancel"), ProcessError]
        public CancelTransactionsResponse Post(CancelTransactions request)
        {
            return _common.CancelTransactions(request);
        }
        [Route("oauth/bets/history"), ValidateTokenData, ProcessError]
        public BetsHistoryResponse Get([FromUri]RoundsHistory request)
        {
            return _common.GetBetHistory(request);
        }
    }
}