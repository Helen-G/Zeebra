using System.Collections.Generic;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;

namespace AFT.RegoV2.GameApi.Interface
{
    public interface IGameApiSettleBetResponse : IGameApiResponse
    {
        decimal Balance { get; set; }
        string CurrencyCode { get; set; }
        List<SettleBetResponseTransaction> Transactions { get; set; }
    }
}
