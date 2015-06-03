using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PHPAnalysis.Tests.Data
{
    internal static class TypeExtensions
    {
        public static IEnumerable<FieldInfo> GetConstants(this Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            return type.GetFields(bindingFlags)
                       .Where(info => info.IsLiteral && !info.IsInitOnly);
        }
    }
}