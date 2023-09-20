using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pancake.ExLib.Reflection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="directDescendants">Select only direct descendants or all subclasses.</param>
        public static IEnumerable<Type> Subclasses(this Type type, bool directDescendants = false)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                foreach (Type t in assembly.DefinedTypes)
                {
                    if ((directDescendants && t.BaseType == type) || t.IsSubclassOf(type))
                    {
                        yield return t;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="directDescendants">Select only direct descendants or all subclasses.</param>
        /// <param name="type"></param>
        public static IEnumerable<Type> Subclasses(this Type type, Assembly assembly, bool directDescendants = false)
        {
            foreach (Type t in assembly.DefinedTypes)
            {
                if ((directDescendants && t.BaseType == type) || t.IsSubclassOf(type))
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Iterate through all the members of the current type and until limit type (not including).
        /// </summary>
        public static IEnumerable<MemberInfo> AllMembers(this Type type, Type limitDescendant)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (MemberInfo memberInfo in typeInfo.DeclaredMembers)
                {
                    yield return memberInfo;
                }
                type = type.BaseType;
            }
            while (type != null && type != limitDescendant);
        }

        /// <summary>
        /// Iterate through all the fields of the current type and until limit type (not including).
        /// </summary>
        public static IEnumerable<FieldInfo> AllFields(this Type type, Type limitDescendant)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (FieldInfo fieldInfo in typeInfo.DeclaredFields)
                {
                    yield return fieldInfo;
                }
                type = type.BaseType;
            }
            while (type != null && type != limitDescendant);
        }

        /// <summary>
        /// Iterate through all the methods of the current type and until limit type (not including).
        /// </summary>
        public static IEnumerable<MethodInfo> AllMethods(this Type type, Type limitDescendant)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (MethodInfo methodInfo in typeInfo.DeclaredMethods)
                {
                    yield return methodInfo;
                }
                type = type.BaseType;
            }
            while (type != null && type != limitDescendant);
        }

        /// <summary>
        /// Iterate through all the properties of the current type and until limit type (not including).
        /// </summary>
        public static IEnumerable<PropertyInfo> AllProperties(this Type type, Type limitDescendant)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (PropertyInfo propertyInfo in typeInfo.DeclaredProperties)
                {
                    yield return propertyInfo;
                }
                type = type.BaseType;
            }
            while (type != null && type != limitDescendant);
        }
        
        /// <summary>
        /// Iterate through all the members of the current type and all base types.
        /// </summary>
        public static IEnumerable<MemberInfo> AllMembers(this Type type)
        {
            return type.AllMembers(null);
        }

        /// <summary>
        /// Iterate through all the fields of the current type and all base types.
        /// </summary>
        public static IEnumerable<FieldInfo> AllFields(this Type type)
        {
            return type.AllFields(null);
        }

        /// <summary>
        /// Iterate through all the methods of the current type and all base types.
        /// </summary>
        public static IEnumerable<MethodInfo> AllMethods(this Type type)
        {
            return type.AllMethods(null);
        }

        /// <summary>
        /// Iterate through all the properties of the current type and all base types.
        /// </summary>
        public static IEnumerable<PropertyInfo> AllProperties(this Type type)
        {
            return type.AllProperties(null);
        }

        /// <summary>
        /// Check if a type provides default constructor?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasDefaultConstructor(this Type type) { return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null; }
        
        private static bool IsSystem(Assembly assembly)
        {
            var assemblyFullName = assembly.FullName;
            if (assemblyFullName.StartsWith("Unity", StringComparison.Ordinal) || assemblyFullName.StartsWith("System", StringComparison.Ordinal) ||
                assemblyFullName.StartsWith("Mono", StringComparison.Ordinal) || assemblyFullName.StartsWith("Accessibility", StringComparison.Ordinal) ||
                assemblyFullName.StartsWith("mscorlib", StringComparison.Ordinal)
                || assemblyFullName.StartsWith("Boo")
                || assemblyFullName.StartsWith("ExCSS")
                || assemblyFullName.StartsWith("I18N")
                || assemblyFullName.StartsWith("nunit.framework")
                || assemblyFullName.StartsWith("ICSharpCode.SharpZipLib")
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