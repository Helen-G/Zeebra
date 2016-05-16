using System;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data.Wallet;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IWalletQueries : IApplicationService
    {
        PlayerBalance GetPlayerBalance(Guid playerId, Guid? walletTemplateId = null);
        Task<PlayerBalance> GetPlayerBalanceAsync(Guid playerId, Guid? walletTemplateId = null);
    }
}
