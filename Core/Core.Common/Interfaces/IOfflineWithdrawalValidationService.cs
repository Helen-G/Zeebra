using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IOfflineWithdrawalValidationService
    {
        void Validate(OfflineWithdrawRequest request);
    }
}