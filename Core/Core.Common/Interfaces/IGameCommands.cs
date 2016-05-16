using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IGameCommands
    {
        void CreateGame(GameDTO game);
        Guid PlaceBet(BetData data, BetContext context, TokenData token);
        Guid WinBet(BetData data, BetContext context);
        Guid LoseBet(BetData data, BetContext context);
        Guid FreeBet(BetData data, BetContext context, TokenData token);
        Guid AdjustTransaction(BetData data, BetContext context);
        Guid CancelTransaction(BetData data, BetContext context);
        void UpdateGame(GameDTO game);
        void UpdateProductSettings(BrandProductSettingsViewModel viewModel);
        void DeleteGame(Guid id);
        void CreateGameServer(GameServer gameServer);
        void UpdateGameServer(GameServer gameServer);
    }
}