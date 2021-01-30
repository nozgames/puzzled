using System;
using System.Collections.Generic;
using System.Reflection;

namespace Puzzled
{
    public static class TypeExtensions  
    {
        private static List<PropertyInfo> _tempProperties = new List<PropertyInfo>(16);

        public static PropertyInfo[] GetFlattenedProperties (this Type type, BindingFlags flags)
        {
            // Add declared only because we will be walking the hierarchy
            flags |= BindingFlags.DeclaredOnly;

            _tempProperties.Clear();
            for (; type != null; type = type.BaseType)
            {
                var tprops = type.GetProperties(flags);
                if (tprops == null || tprops.Length == 0)
                    continue;

                _tempProperties.AddRange(tprops);
            }                
            return _tempProperties.ToArray();
        }
    }
}
