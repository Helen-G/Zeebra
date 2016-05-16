using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ITransferFundCommands : IApplicationService
    {
        string AddFund(TransferFundRequest request);
    }
}
