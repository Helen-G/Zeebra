using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Roles;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Security.Helpers;
using AFT.RegoV2.Domain.Security;
using AFT.RegoV2.Domain.Security.Events;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;

namespace AFT.RegoV2.Domain.BoundedContexts.Security.ApplicationServices
{
    public class RoleService : MarshalByRefObject, IApplicationService
    {
        private readonly ISecurityRepository _repository;
        private readonly ISecurityProvider _security;
        private readonly IEventBus _eventBus;

        public RoleService(
            ISecurityRepository repository, 
            ISecurityProvider security,
            IEventBus eventBus)
        {
            _repository = repository;
            _security = security;
            _eventBus = eventBus;
        }

        #region Queries
        public Role GetRoleById(Guid roleId, bool includePermissions = false)
        {
            var query = _repository.Roles.Include(r => r.CreatedBy)
                .Include(r => r.UpdatedBy).Include(r => r.Licensees)
                .AsQueryable();

            if (includePermissions)
            {
                query = query.Include(r => r.Permissions);
            }

            return query.SingleOrDefault(r => r.Id == roleId);
        }

        [Permission(Permissions.View, Module = Modules.RoleManager)]
        [Permission(Permissions.View, Module = Modules.AdminManager)]
        public IQueryable<Role> GetRoles()
        {
            return _repository.Roles.Include(x => x.CreatedBy)
                .Include(r => r.UpdatedBy).Include(r => r.Licensees)
                .Include(r => r.Licensees)
                .AsQueryable();
        }

        #endregion

        [Permission(Permissions.Add, Module = Modules.RoleManager)]
        public Role CreateRole(AddRoleData data)
        {
            var role = Mapper.DynamicMap<Role>(data);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                role.Id = Guid.NewGuid();
                role.CreatedBy = _repository.Users.SingleOrDefault(u => u.Id == _security.User.UserId);
                role.CreatedDate = DateTimeOffset.UtcNow;

                SetRolePermissions(role, data.CheckedPermissions);

                role.SetLicensees(data.AssignedLicensees);

                _repository.Roles.Add(role);

                _repository.SaveChanges();

                _eventBus.Publish(new RoleCreated(role));

                scope.Complete();
            }

            return role;
        }

        private void SetRolePermissions(Role role, IEnumerable<Guid> permissionIds)
        {
            if (permissionIds == null) return;

            role.Permissions.Clear();

            var permissionsToDelete = new List<RolePermission>();

            foreach (var rolePermission in _repository.RolePermissions)
            {
                if (rolePermission.RoleId == role.Id)
                {
                    permissionsToDelete.Add(rolePermission);
                }
            }

            foreach (var permission in permissionsToDelete)
            {
                _repository.RolePermissions.Remove(permission);
            }

            foreach (var permissionId in permissionIds)
            {
                var permission = _repository.Permissions.FirstOrDefault(o => o.Id == permissionId);

                if (permission == null)
                {
                    throw new RegoException(string.Format("Operation with id: {0} not found", permissionId));
                }

                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                };

                role.Permissions.Add(rolePermission);
            }
        }

        [Permission(Permissions.Edit, Module = Modules.RoleManager)]
        public Role UpdateRole(EditRoleData data)
        {
            var role = GetRoleById(data.Id, true);
            if (role == null)
                throw new RegoException("Role not found");

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                role.Code = data.Code;
                role.Name = data.Name;
                role.Description = data.Description;
                role.UpdatedBy = _repository.Users.Single(u => u.Id == _security.User.UserId);
                role.UpdatedDate = DateTimeOffset.UtcNow;

                SetRolePermissions(role, data.CheckedPermissions);

                role.SetLicensees(data.AssignedLicensees);

                _repository.SaveChanges();

                _eventBus.Publish(new RoleUpdated(role));

                scope.Complete();

                return role;                
            }
        }
    }
}
