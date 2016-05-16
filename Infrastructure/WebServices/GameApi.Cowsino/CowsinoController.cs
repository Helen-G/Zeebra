using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AFT.RegoV2.GameApi.Interface.Attributes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.Attributes;

namespace AFT.RegoV2.GameApi.Dopamine.Controllers
{
    [ForceJsonFormatter]
    public class CowsinoController : ApiController
    {
        private readonly IGamesCommonOperationsProvider _common;

        public CowsinoController(IGamesCommonOperationsProvider common)
        {
            _common = common;
        }

        [Route("api/cowsino/token/validate"), ValidateTokenData, ProcessError]
        public ValidateTokenResponse Post(ValidateToken request)
        {
            return _common.ValidateToken(request);
        }
        [Route("api/cowsino/players/balance"), ValidateTokenData, ProcessError]
        public GetBalanceResponse Get([FromUri]GetBalance request)
        {
            return _common.GetBalance(request);
        }
        [Route("api/cowsino/bets/place"), ValidateTokenData, ProcessError]
        public async Task<PlaceBetResponse> Post(PlaceBet request)
        {
            return await _common.PlaceBet(request);
        }
        [Route("api/cowsino/bets/win"), ValidateTokenData, ProcessError]
        public WinBetResponse Post(WinBet request)
        {
            return _common.WinBet(request);
        }
        [Route("api/cowsino/bets/lose"), ValidateTokenData, ProcessError]
        public LoseBetResponse Post(LoseBet request)
        {
            return _common.LoseBet(request);
        }
        [Route("api/cowsino/bets/freebet"), ValidateTokenData, ProcessError]
        public FreeBetResponse Post(FreeBet request)
        {
            return _common.FreeBet(request);
        }
        [Route("api/cowsino/transactions/adjust"), ValidateTokenData, ProcessError]
        public AdjustTransactionResponse Post(AdjustTransaction request)
        {
            return _common.AdjustTransaction(request);
        }
        [Route("api/cowsino/transactions/cancel"), ValidateTokenData, ProcessError]
        public CancelTransactionResponse Post(CancelTransaction request)
        {
            return _common.CancelTransaction(request);
        }
        [Route("api/cowsino/batch/bets/settle"), ProcessError]
        public async Task<SettleBetsResponse> Post(SettleBets request)
        {
            return await _common.SettleBets(request);
        }
        [Route("api/cowsino/batch/transactions/adjust"), ProcessError]
        public AdjustTransactionsResponse Post(AdjustTransactions request)
        {
            return _common.AdjustTransactions(request);
        }
        [Route("api/cowsino/batch/transactions/cancel"), ProcessError]
        public CancelTransactionsResponse Post(CancelTransactions request)
        {
            return _common.CancelTransactions(request);
        }
        [Route("api/cowsino/bets/history"), ValidateTokenData, ProcessError]
        public BetsHistoryResponse Get([FromUri]RoundsHistory request)
        {
            return _common.GetBetHistory(request);
        }

    }
}

