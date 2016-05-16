using System;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IPermissionService : IApplicationService
    {
        bool VerifyPermission(Guid userId, string operationName, string operationParent = "Root");
        void AddBrandToUser(Guid userId, Guid brandId);
    }
}