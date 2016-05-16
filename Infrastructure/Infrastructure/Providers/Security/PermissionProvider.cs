using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Infrastructure.Constants;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Providers
{
    public class PermissionProvider : IPermissionProvider
    {
        private IList<Permission> _operations;

        public IList<Permission> Operations
        {
            get { return _operations ?? (_operations = BuildPermissionsTree()); }
        }

        public IEnumerable<Permission> GetPermissions()
        {
            return Operations;
        }

        public Permission GetRootPermission()
        {
            return Operations.FirstOrDefault(o => o.Parent == null && o.Name == SecurityConstants.RootName);
        }

        public Permission GetPermission(string name, string parent = null)
        {
            return Operations.FirstOrDefault(o => o.Parent != null && o.Parent.Name == (parent ?? SecurityConstants.RootName) && o.Name == name);
        }

        public bool RegisterPermissionUnlessExists(string name, string category)
        {
            var root = Operations.FirstOrDefault(o => o.Name == "Root" && o.Parent == null);

            if (root == null)
            {
                throw new RegoException("Root role not found");
            }

            var parent = Operations.FirstOrDefault(o => o.Name == category);
            if (parent == null)
            {
                parent = new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = category,
                    ParentId = root.Id,
                    Parent = root
                };
                Operations.Add(parent);
            }

            var operation = new Permission
            {
                Id = Guid.NewGuid(),
                Name = name,
                ParentId = parent.Id,
                Parent = parent
            };

            if (Operations.Any(o => o.Compare(operation))) return false;

            Operations.Add(operation);

            return true;
        }

        private static MethodInfo GetRealMethod(Type targetType, MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType == null)
                return null;

            var map = targetType.GetInterfaceMap(methodInfo.DeclaringType);
            var index = Array.IndexOf(map.InterfaceMethods, methodInfo);

            if (index == -1)
            {
                //something's wrong;
            }

            return map.TargetMethods[index];
        }


        private static IList<Permission> BuildPermissionsTree()
        {
            var operations = new List<Permission>();

            var root = new Permission
            {
                Id = Guid.NewGuid(),
                Name = SecurityConstants.RootName
            };

            operations.Add(root);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("AFT.RegoV2.") && !x.FullName.StartsWith("AFT.RegoV2.Infrastructure"));

            foreach (var service in loadedAssemblies.SelectMany(x => x.GetLoadableTypes()).Where(t => typeof(MarshalByRefObject).IsAssignableFrom(t)))
            {
                foreach (var method in service.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var operationAttrs = method.GetCustomAttributes(typeof(PermissionAttribute), true);

                    foreach (var operationAttr in operationAttrs.Select(o => (PermissionAttribute)o))
                    {
                        var parent = operations.FirstOrDefault(p => p.Name == operationAttr.Module);

                        if (!string.IsNullOrWhiteSpace(operationAttr.Module) && parent == null)
                        {
                            parent = new Permission()
                            {
                                Id = Guid.NewGuid(),
                                Name = operationAttr.Module,
                                ParentId = root.Id,
                                Parent = root
                            };

                            operations.Add(parent);
                        }

                        var permission = new Permission()
                        {
                            Id = Guid.NewGuid(),
                            Name = operationAttr.Permission,
                            ParentId = parent != null ? parent.Id : root.Id,
                            Parent = parent ?? root
                        };

                        if (!operations.Any(o => o.Name == permission.Name && o.Parent != null && o.Parent.Name == permission.Parent.Name))
                            operations.Add(permission);
                    }
                }
            }

            return operations;
        }

        public IEnumerable<Permission> BuildPermissionsList(IEnumerable<Permission> rootPermissions)
        {
            var result = new List<Permission>();

            result.AddRange(rootPermissions);

            foreach (var operation in rootPermissions)
            {
                operation.Parent = GetPermissions().SingleOrDefault(o => o.Id == operation.ParentId);

                var children = GetPermissions().Where(o => o.ParentId == operation.Id).ToList();

                result.AddRange(children);

                var subChildren = BuildPermissionsList(children);

                result.AddRange(subChildren);
            }

            return result;
        }

        public RoleOperation CreateRolePermission(Permission permission)
        {
            if (permission == null)
            {
                return null;
            }
            var result = new RoleOperation
            {
                Id = Guid.NewGuid(),
                OperationName = string.Join("_", permission.Name, permission.Parent != null ? permission.Parent.Name : null).Trim('_')
            };

            return result;
        }

        public IEnumerable<RoleOperation> CreateRolePermissions(IEnumerable<Permission> operations)
        {
            return operations.Select(CreateRolePermission);
        }

        public IEnumerable<Permission> GetPermissionsFromRole(IEnumerable<RoleOperation> rolePermissions)
        {
            var operations = new List<Permission>();

            foreach (var roleOperation in rolePermissions)
            {
                var operationName = roleOperation.OperationName.Split('_');
                var name = operationName.First();
                string category = null;
                if (operationName.Count() > 1)
                    category = operationName.Last();

                var operation =
                    Operations.SingleOrDefault(
                        o => o.Name == name && (category == null || o.Parent == null || o.Parent.Name == category));
                if (operation != null)
                    operations.Add(operation);
            }

            return BuildPermissionsList(operations);
        }


        public RoleOperation GetRolePermission(string name, string parent = null)
        {
            return CreateRolePermission(GetPermission(name, parent));
        }
    }
}
