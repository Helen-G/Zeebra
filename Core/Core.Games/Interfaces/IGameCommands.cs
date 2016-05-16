using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameCommands : IApplicationService
    {
        Guid PlaceBet(GameActionData data, GameActionContext context, TokenData token);
        Guid WinBet(GameActionData data, GameActionContext context);
        Guid LoseBet(GameActionData data, GameActionContext context);
        Guid FreeBet(GameActionData data, GameActionContext context, TokenData token);
        Guid AdjustTransaction(GameActionData data, GameActionContext context);
        Guid CancelTransaction(GameActionData data, GameActionContext context);

        Task<Guid> WinBetAsync(GameActionData data, GameActionContext context);
        Task<Guid> LoseBetAsync(GameActionData data, GameActionContext context);

    }
}