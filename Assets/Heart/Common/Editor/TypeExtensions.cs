using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Linq;
using UnityEditor;

namespace PancakeEditor.Common
{
    public static class TypeExtensions
    {
        public const BindingFlags MAX_BINDING_FLAGS = (BindingFlags) 62;

        public static readonly MethodInfo GUIViewMarkHotRegion =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView").GetMethod("MarkHotRegion", MAX_BINDING_FLAGS);

        public static readonly PropertyInfo CurrentGuiView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView").GetProperty("current", MAX_BINDING_FLAGS);

        public static readonly MethodInfo GUIClipUnclipToWindow = typeof(UnityEngine.GUI).Assembly.GetType("UnityEngine.GUIClip")
            .GetMethod("UnclipToWindow",
                MAX_BINDING_FLAGS,
                null,
                new[] {typeof(UnityEngine.Rect)},
                null);

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfoCache = new();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyInfoCache = new();
        private static readonly Dictionary<Type, Dictionary<int, MethodInfo>> MethodInfoCache = new();
        private static readonly Dictionary<string, Type> TypeCache = new();

        public static Type InspectorWindow => typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        public static Type PropertyEditor => typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        public static Type GameObjectInspector => typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");


        public static object GetFieldValue(this object o, string fieldName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetFieldInfo(fieldName) is { } fieldInfo) return fieldInfo.GetValue(target);

            if (exceptionIfNotFound) throw new Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static object GetPropertyValue(this object o, string propertyName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is { } propertyInfo) return propertyInfo.GetValue(target);


            if (exceptionIfNotFound) throw new Exception($"Property '{propertyName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static object GetMemberValue(this object o, string memberName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetFieldInfo(memberName) is { } fieldInfo) return fieldInfo.GetValue(target);

            if (type.GetPropertyInfo(memberName) is { } propertyInfo) return propertyInfo.GetValue(target);

            if (exceptionIfNotFound) throw new Exception($"Member '{memberName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static void SetFieldValue(this object o, string fieldName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetFieldInfo(fieldName) is { } fieldInfo)
                fieldInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");
        }

        public static void SetPropertyValue(this object o, string propertyName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is { } propertyInfo)
                propertyInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new Exception($"Property '{propertyName}' not found in type '{type.Name}' and its parent types");
        }

        public static void SetMemberValue(this object o, string memberName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetFieldInfo(memberName) is { } fieldInfo)
                fieldInfo.SetValue(target, value);

            else if (type.GetPropertyInfo(memberName) is { } propertyInfo)
                propertyInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new Exception($"Member '{memberName}' not found in type '{type.Name}' and its parent types");
        }

        public static object InvokeMethod(this object o, string methodName, params object[] parameters)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetMethodInfo(methodName, parameters.Select(r => r.GetType()).ToArray()) is { } methodInfo) return methodInfo.Invoke(target, parameters);

            throw new Exception($"Method '{methodName}' not found in type '{type.Name}', its parent types and interfaces");
        }

        private static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            if (FieldInfoCache.TryGetValue(type, out var fieldInfosByNames))
            {
                if (fieldInfosByNames.TryGetValue(fieldName, out var fieldInfo)) return fieldInfo;
            }


            if (!FieldInfoCache.ContainsKey(type)) FieldInfoCache[type] = new Dictionary<string, FieldInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetField(fieldName, MAX_BINDING_FLAGS) is { } fieldInfo) return FieldInfoCache[type][fieldName] = fieldInfo;
            }


            return FieldInfoCache[type][fieldName] = null;
        }

        private static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            if (PropertyInfoCache.TryGetValue(type, out var propertyInfosByNames))
            {
                if (propertyInfosByNames.TryGetValue(propertyName, out var propertyInfo)) return propertyInfo;
            }

            if (!PropertyInfoCache.ContainsKey(type)) PropertyInfoCache[type] = new Dictionary<string, PropertyInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetProperty(propertyName, MAX_BINDING_FLAGS) is { } propertyInfo) return PropertyInfoCache[type][propertyName] = propertyInfo;
            }


            return PropertyInfoCache[type][propertyName] = null;
        }

        private static MethodInfo GetMethodInfo(this Type type, string methodName, params Type[] argumentTypes)
        {
            int methodHash = methodName.GetHashCode() ^ argumentTypes.Aggregate(0, (hash, r) => hash ^ r.GetHashCode());

            if (MethodInfoCache.TryGetValue(type, out var methodInfosByHashes))
            {
                if (methodInfosByHashes.TryGetValue(methodHash, out var methodInfo)) return methodInfo;
            }

            if (!MethodInfoCache.ContainsKey(type)) MethodInfoCache[type] = new Dictionary<int, MethodInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetMethod(methodName,
                        MAX_BINDING_FLAGS,
                        null,
                        argumentTypes,
                        null) is { } methodInfo) return MethodInfoCache[type][methodHash] = methodInfo;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.GetMethod(methodName,
                        MAX_BINDING_FLAGS,
                        null,
                        argumentTypes,
                        null) is { } methodInfo) return MethodInfoCache[type][methodHash] = methodInfo;
            }

            return MethodInfoCache[type][methodHash] = null;
        }


        public static T GetFieldValue<T>(this object o, string fieldName, bool exceptionIfNotFound = true) => (T) o.GetFieldValue(fieldName, exceptionIfNotFound);

        public static T GetPropertyValue<T>(this object o, string propertyName, bool exceptionIfNotFound = true) =>
            (T) o.GetPropertyValue(propertyName, exceptionIfNotFound);

        public static T GetMemberValue<T>(this object o, string memberName, bool exceptionIfNotFound = true) => (T) o.GetMemberValue(memberName, exceptionIfNotFound);
        public static T InvokeMethod<T>(this object o, string methodName, params object[] parameters) => (T) o.InvokeMethod(methodName, parameters);


        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="directDescendants">Select only direct descendants or all subclasses.</param>
        public static IEnumerable<Type> Subclasses(this Type type, bool directDescendants = false)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                foreach (var t in assembly.DefinedTypes)
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
        /// <param name="targetType"></param>
        /// <param name="filter"></param>
        public static List<Type> GetAllSubClass<T>(this Type targetType, Predicate<Type> filter = null)
        {
            var list = UnityEditor.TypeCache.GetTypesDerivedFrom<T>().ToList();
            return list.Filter(type => SubclassFilter(type) && (filter == null || filter(type)));
            bool SubclassFilter(Type type) => type.IsClass && type.IsSubclassOf(targetType);
        }

        public static void FindTypeByFullName(string name, out Type type)
        {
            if (TypeCache.TryGetValue(name, out type)) return;

            type = Type.GetType(name);
            if (type != null)
            {
                TypeCache.TryAdd(name, type);
                return;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type != null)
                {
                    TypeCache.TryAdd(name, type);
                    return;
                }
            }
        }
    }
}