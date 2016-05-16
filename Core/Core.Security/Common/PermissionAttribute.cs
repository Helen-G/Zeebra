using System;

namespace AFT.RegoV2.ApplicationServices.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : Attribute
    {
        public string Permission { get; set; }
        public string Module { get; set; }

        public PermissionAttribute()
        {
        }

        public PermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public PermissionAttribute(string permission, string module)
            : this(permission)
        {
            Module = module;
        }
    }
}