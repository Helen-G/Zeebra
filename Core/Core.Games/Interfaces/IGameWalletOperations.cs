using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Game.Interfaces
{
    public interface IGameWalletOperations : IApplicationService
    {
        Guid PlaceBet(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Guid CancelBet(Guid playerId, Guid gameId, Guid transactionId);

        Guid WinBet(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Task<Guid> WinBetAsync(Guid playerId, Guid gameId, Guid roundId, decimal amount);

        Guid LoseBet(Guid playerId, Guid gameId, Guid roundId);

        Guid FreeBet(Guid playerId, Guid gameId, decimal amount);

        Guid AdjustBetTransaction(Guid playerId, Guid gameId, Guid transactionId, decimal newAmount);
    }
}
