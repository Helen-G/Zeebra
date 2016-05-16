using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IWagerConfigurationCommands : IApplicationService
    {
        void ActivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId);
        Guid CreateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId);
        void DeactivateWagerConfiguration(WagerConfigurationId wagerId, Guid userId);
        Guid UpdateWagerConfiguration(WagerConfigurationDTO wagerConfigurationDTO, Guid userId);
    }
}