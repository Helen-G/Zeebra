using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AFT.RegoV2.Infrastructure.Constants
{
    public static class SecurityConstants
    {
        public const string RootName = "Root";
    }

    public static class ConstantsHelper
    {
        public static Dictionary<string, string> GetConstantsDictionary<T>()
        {
            var type = typeof(T);

            var fieldInfos = type.GetFields(
                BindingFlags.Public | BindingFlags.Static |

                // This tells it to get the fields from all base types as well
                BindingFlags.FlattenHierarchy);

            var dictionary = fieldInfos
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .ToDictionary(fi => fi.Name, fi => fi.GetRawConstantValue().ToString());

            return dictionary;
        }
    }

    public class IpRegulationConstants
    {
        public class BlockingTypes
        {
            public const string Redirection = "Redirection";
            public const string LoginRegistration = "Login/Registration";
        }
    }

    public class RoleConstants
    {
        public class SuperAdmin
        {
            public static readonly Guid Id = new Guid("00000000-0000-0000-0000-000000000001");
            public static readonly string Name = "SuperAdmin";
        }
    }


}
