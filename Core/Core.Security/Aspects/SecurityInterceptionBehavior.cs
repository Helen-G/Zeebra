using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace AFT.RegoV2.Core.Security.Aspects
{
    public class SecurityInterceptionBehavior : IInterceptionBehavior
    {
        private readonly IPermissionService _permissionService;
        private readonly ISecurityProvider _securityProvider;

        public SecurityInterceptionBehavior(
            IPermissionService permissionService,
            ISecurityProvider securityProvider)
        {
            _permissionService = permissionService;

            _securityProvider = securityProvider;
        }

        public IMethodReturn Invoke(IMethodInvocation input,
          GetNextInterceptionBehaviorDelegate getNext)
        {
            var methodBaseClass = input.MethodBase;
            if (input.MethodBase.DeclaringType != null && input.MethodBase.DeclaringType.IsInterface)
            {
                // Map interface method to implementing class method which contains the Permission attributes
                var targetType = input.Target.GetType();
                var parameterTypes = input.MethodBase.GetParameters().Select(par => par.ParameterType).ToArray();
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
                methodBaseClass = targetType.GetMethod(input.MethodBase.ReflectedType.FullName + "." + input.MethodBase.Name, flags, Type.DefaultBinder, parameterTypes, null) ??
                    targetType.GetMethod(input.MethodBase.Name, flags, Type.DefaultBinder, parameterTypes, null) ??
                        input.MethodBase;
            }

            var permissionAttrs = methodBaseClass.GetCustomAttributes(typeof(PermissionAttribute), true);

            if (permissionAttrs.Any())
            {
                if (_securityProvider.Session == null || _securityProvider.User == null)
                    throw new RegoException("Session has expired or current user data is not accessible.");

                var allowed = false;
                var userName = "[not logged in]";
                if (_securityProvider.User != null)
                {
                    userName = _securityProvider.User.UserName;
                    var userId = _securityProvider.User.UserId;
                    allowed = permissionAttrs.Select(p => (PermissionAttribute)p).Aggregate(false, (current, permission) => current
                                                                   || _permissionService.VerifyPermission(userId,
                                                                       permission.Permission, permission.Module));
                }

                if (!allowed)
                    throw new InsufficientPermissionsException(
                        string.Format("User \"{0}\" has insufficient permissions for the operation ", userName));
            }

            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }

        private void WriteLog(string message)
        {

        }
    }
}
