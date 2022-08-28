#if PANCAKE_REFLECTION
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Pancake.Core
{
    using System;

    public static partial class Util
    {
        /// <summary>
        /// Get attribute type <typeparamref name="T"/> from <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            var attributeType = typeof(T);
            if (!type.IsDefined(attributeType, inherit)) return null;
            return (T)Attribute.GetCustomAttribute(type, attributeType);
        }

        /// <summary>
        /// Indicates that <paramref name="type"/> has attribute <typeparamref name="T"/>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasAttribute<T>(this Type type, bool inherit) { return type.IsDefined(typeof(T), inherit); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
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

        public static List<Type> GetAllSubTypes(Type targetType, Predicate<Type> filter = null)
        {
            bool SubclassFilter(Type type) => type.IsClass && !type.IsAbstract && type.IsSubclassOf(targetType);
            return filter == null ? GetTypes(SubclassFilter) : GetTypes(type => SubclassFilter(type) && filter(type));
        }

        public static T Clone<T>(T @object) where T : class
        {
            if (@object == null) return null;
            var method = @object.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) return null;
            return (T)method.Invoke(@object, null);
        }

        public static long Measure(string operation, Action action, bool printResult = true)
        {
            var watch = Stopwatch.StartNew();

            action();
            watch.Stop();

            var result = watch.ElapsedMilliseconds;

            if (printResult) UnityEngine.Debug.Log(operation + ": " + result);
            return result;
        }

        public static List<Type> GetAllImplementations(Type interfaceType, Predicate<Type> filter = null)
        {
            bool SubclassFilter(Type type) => type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type);
            return filter == null ? GetTypes(SubclassFilter) : GetTypes(type => SubclassFilter(type) && filter(type));
        }

        public static void Catch(ref Exception exception, Action action, Action finallyAction = null)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (exception == null) exception = e;
            }
            finally
            {
                if (finallyAction != null) finallyAction();
            }
        }

        public static bool IsAssignable(Type fieldType, Type targetType)
        {
            if (targetType == typeof(UnityEngine.Object)) return targetType.IsAssignableFrom(fieldType);
            bool isAssignableFrom = fieldType.IsAssignableFrom(targetType);
            return isAssignableFrom;
        }
    }
}
#endif