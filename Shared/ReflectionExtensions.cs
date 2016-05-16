using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AFT.RegoV2.Shared
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static IEnumerable<Type> GetRegoTypes(this AppDomain appDomain)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("AFT.RegoV2."))
                .SelectMany(s => s.GetLoadableTypes());
        }

        /// <summary>
        /// Determines if thisType is inherited directly or indirectly from instance 
        /// </summary>
        public static bool IsDescendentOf(this Type thisType, Type type)
        {
            return type.IsAssignableFrom(thisType);
        }
    }
}
