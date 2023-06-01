using System;
using System.Collections.Generic;
using System.Reflection;

namespace PancakeEditor
{
    public static partial class Editor
    {
        private static bool IsSystem(Assembly assembly)
        {
            var assemblyFullName = assembly.FullName;
            if (assemblyFullName.StartsWith("Unity", StringComparison.Ordinal) || assemblyFullName.StartsWith("System", StringComparison.Ordinal) ||
                assemblyFullName.StartsWith("Mono", StringComparison.Ordinal) || assemblyFullName.StartsWith("Accessibility", StringComparison.Ordinal) ||
                assemblyFullName.StartsWith("mscorlib", StringComparison.Ordinal)
                // || assemblyFullName.StartsWith("Boo")
                // || assemblyFullName.StartsWith("ExCSS")
                // || assemblyFullName.StartsWith("I18N")
                // || assemblyFullName.StartsWith("nunit.framework")
                // || assemblyFullName.StartsWith("ICSharpCode.SharpZipLib")
               ) return true;

            return false;
        }

        public static List<Type> GetTypes(Predicate<Type> filter = null)
        {
            var result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsSystem(assembly)) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                if (filter == null)
                {
                    for (var i = 0; i < types.Length; i++) result.Add(types[i]);
                }
                else
                {
                    for (var i = 0; i < types.Length; i++)
                    {
                        var type = types[i];
                        if (filter(type)) result.Add(type);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// retrurn all sub type without abstract type
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static List<Type> GetAllSubClassNoAbstract(this Type targetType, Predicate<Type> filter = null)
        {
            bool SubclassFilter(Type type) => type.IsClass && !type.IsAbstract && type.IsSubclassOf(targetType);
            return filter == null ? GetTypes(SubclassFilter) : GetTypes(type => SubclassFilter(type) && filter(type));
        }

        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="filter"></param>
        public static List<Type> GetAllSubClass(this Type targetType, Predicate<Type> filter = null)
        {
            bool SubclassFilter(Type type) => type.IsClass && type.IsSubclassOf(targetType);
            return filter == null ? GetTypes(SubclassFilter) : GetTypes(type => SubclassFilter(type) && filter(type));
        }

        /// <summary>
        /// Iterate through all the members of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<MemberInfo> AllMembers(this Type type)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (MemberInfo memberInfo in typeInfo.DeclaredMembers)
                {
                    yield return memberInfo;
                }

                type = type.BaseType;
            } while (type != null);
        }

        public static bool TryFindTypeByFullName(string name, out Type type)
        {
            type = Type.GetType(name);
            if (type != null) return true;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null) return true;
            }

            return false;
        }
    }
}