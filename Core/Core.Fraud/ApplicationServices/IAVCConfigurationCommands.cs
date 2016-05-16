using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public interface IAVCConfigurationCommands : IApplicationService
    {
        void Create(AVCConfigurationDTO data);
        void Update(AVCConfigurationDTO data);
        void Delete(Guid id);
    }
}
