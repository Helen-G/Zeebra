using System.Collections.Generic;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;

namespace AFT.RegoV2.GameApi.Interface
{
    public interface IGameApiSettleBetRequest : IGameApiRequest
    {
        List<SettleBetTransaction> Transactions { get; set; } 
    }
}
