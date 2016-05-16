using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Shared.Data;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IBasePaymentQueries : IApplicationService
    {
        IEnumerable<PaymentLevelDTO> GetPaymentLevels();
        PaymentSettingDTO GetPaymentSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel, PaymentType type);
        TransferSettingDTO GetTransferSetting(Guid brandId, string currencyCode, VipLevelViewModel vipLevel);
        TransferSettingDTO GetTransferSetting(string productId, TransferFundType transferFundType, bool enabled);
    }
}