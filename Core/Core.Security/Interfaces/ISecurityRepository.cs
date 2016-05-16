using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;

namespace AFT.RegoV2.Core.Services.Security
{
    public interface ISecurityRepository
    {
        IDbSet<Role> Roles { get; }
        IDbSet<User> Users { get; }
        IDbSet<AdminIpRegulation> AdminIpRegulations { get; }
        IDbSet<BrandIpRegulation> BrandIpRegulations { get; }
        IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings { get; }
        IDbSet<RolePermission> RolePermissions { get; }
        IDbSet<Error> Errors { get; }
        IDbSet<Session> Sessions { get; }
        IDbSet<Permission> Permissions { get; }
        User GetUserById(Guid userId);
        int SaveChanges();
    }
}