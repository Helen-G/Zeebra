using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeSecurityRepository : ISecurityRepository
    {
        private readonly FakeDbSet<User> _users = new FakeDbSet<User>();
        private readonly FakeDbSet<Role> _roles = new FakeDbSet<Role>();
        private readonly FakeDbSet<AdminIpRegulation> _adminIpRegulations = new FakeDbSet<AdminIpRegulation>();
        private readonly FakeDbSet<BrandIpRegulation> _brandIpRegulations = new FakeDbSet<BrandIpRegulation>();
        private readonly FakeDbSet<AdminIpRegulationSetting> _adminIpRegulationSettings = new FakeDbSet<AdminIpRegulationSetting>();
        private readonly FakeDbSet<RolePermission> _roleOperations = new FakeDbSet<RolePermission>();
        private readonly FakeDbSet<Error> _errors = new FakeDbSet<Error>();
        private readonly FakeDbSet<Session> _sessions = new FakeDbSet<Session>();
        private readonly FakeDbSet<Permission> _permissions = new FakeDbSet<Permission>(); 

        public  IDbSet<User> Users
        {
            get { return _users; }
        }

        public  IDbSet<Role> Roles
        {
            get { return _roles; }
        }

        public  IDbSet<RolePermission> RolePermissions
        {
            get { return _roleOperations; }
        }

        public IDbSet<AdminIpRegulation> AdminIpRegulations
        {
            get { return _adminIpRegulations; }
        }

        public IDbSet<AdminIpRegulationSetting> AdminIpRegulationSettings
        {
            get { return _adminIpRegulationSettings; }
        }

        public IDbSet<BrandIpRegulation> BrandIpRegulations
        {
            get { return _brandIpRegulations; }
        }

        public IDbSet<Error> Errors
        {
            get { return _errors; }
        }

        public IDbSet<Session> Sessions 
        {
            get { return _sessions; }
        }

        public IDbSet<Permission> Permissions
        {
            get { return _permissions; }
        }

        public User GetUserById(Guid userId)
        {
            return _users.Single(u => u.Id == userId);
        }

        public int SaveChanges()
        {
            return 1;
        }

        public void SetDetached(object entity)
        {
        }
    }
}
