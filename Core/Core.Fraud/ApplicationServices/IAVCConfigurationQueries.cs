using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IAVCConfigurationQueries : IApplicationService
    {
        #region Public methods

        AVCConfigurationDTO GetAutoVerificationCheckConfiguration(Guid id);
        IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations();
        IEnumerable<AutoVerificationCheckConfiguration> GetAutoVerificationCheckConfigurations(Guid brandId);

        #endregion
    }
}