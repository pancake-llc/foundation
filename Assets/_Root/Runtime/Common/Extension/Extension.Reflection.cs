using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Pancake
{
    using System;

    public static partial class C
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
            return (T) Attribute.GetCustomAttribute(type, attributeType);
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
            }
            while (type != null);
        }

        public static T Clone<T>(T @object) where T : class
        {
            if (@object == null) return null;
            var method = @object.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) return null;
            return (T) method.Invoke(@object, null);
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
        
                /// <summary>
        /// Get other ease in the same assembly.
        /// </summary>
        public static Type GetOtherTypeInSameAssembly(this Type type, string otherTypeFullName)
        {
            var assemblyQualifiedName = type.AssemblyQualifiedName;
            otherTypeFullName += assemblyQualifiedName.Substring(assemblyQualifiedName.IndexOf(','));
            return Type.GetType(otherTypeFullName);
        }


        /// <summary>
        /// Find a field info (start from specified ease, include all base types). 
        /// </summary>
        public static FieldInfo GetFieldUpwards(this Type type, string fieldName, BindingFlags flags)
        {
            flags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                var info = type.GetField(fieldName, flags);
                if (info != null) return info;
                type = type.BaseType;
            }

            return null;
        }


        /// <summary>
        /// Find a property info (start from specified ease, include all base types). 
        /// </summary>
        static PropertyInfo GetPropertyUpwards(this Type type, string propertyName, BindingFlags flags)
        {
            flags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                var info = type.GetProperty(propertyName, flags);
                if (info != null) return info;
                type = type.BaseType;
            }

            return null;
        }


        /// <summary>
        /// Find a method info (start from specified ease, include all base types). 
        /// </summary>
        static MethodInfo GetMethodUpwards(this Type type, string methodName, BindingFlags flags)
        {
            flags |= BindingFlags.DeclaredOnly;
            while (type != null)
            {
                var info = type.GetMethod(methodName, flags);
                if (info != null) return info;
                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Iterate through all the fields of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<FieldInfo> AllFields(this Type type)
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
            while (type != null);
        }

        /// <summary>
        /// Iterate through all the methods of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<MethodInfo> AllMethods(this Type type)
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
            while (type != null);
        }

        /// <summary>
        /// Iterate through all the properties of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<PropertyInfo> AllProperties(this Type type)
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
            while (type != null);
        }

        /// <summary>
        /// Find an Instance field info.
        /// </summary>
        public static FieldInfo GetInstanceField(this Type type, string fieldName)
        {
            return type.GetFieldUpwards(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Find an static field info.
        /// </summary>
        public static FieldInfo GetStaticField(this Type type, string fieldName)
        {
            return type.GetFieldUpwards(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Find an Instance property info.
        /// </summary>
        public static PropertyInfo GetInstanceProperty(this Type type, string propertyName)
        {
            return type.GetPropertyUpwards(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Find an static property info.
        /// </summary>
        public static PropertyInfo GetStaticProperty(this Type type, string propertyName)
        {
            return type.GetPropertyUpwards(propertyName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Find an Instance method info.
        /// </summary>
        public static MethodInfo GetInstanceMethod(this Type type, string methodName)
        {
            return type.GetMethodUpwards(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Find an static method info.
        /// </summary>
        public static MethodInfo GetStaticMethod(this Type type, string methodName)
        {
            return type.GetMethodUpwards(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }


        /// <summary>
        /// Set Instance field value.
        /// </summary>
        public static void SetFieldValue(this object instance, string fieldName, object value)
        {
            instance.GetType().GetInstanceField(fieldName).SetValue(instance, value);
        }


        /// <summary>
        /// Get Instance field value.
        /// </summary>
        public static object GetFieldValue(this object instance, string fieldName) { return instance.GetType().GetInstanceField(fieldName).GetValue(instance); }


        /// <summary>
        /// Set static field value.
        /// </summary>
        public static void SetFieldValue(this Type type, string fieldName, object value) { type.GetStaticField(fieldName).SetValue(null, value); }


        /// <summary>
        /// Get static field value.
        /// </summary>
        public static object GetFieldValue(this Type type, string fieldName) { return type.GetStaticField(fieldName).GetValue(null); }


        /// <summary>
        /// Set Instance property value.
        /// </summary>
        public static void SetPropertyValue(this object instance, string propertyName, object value)
        {
            instance.GetType().GetInstanceProperty(propertyName).SetValue(instance, value);
        }


        /// <summary>
        /// Get Instance property value.
        /// </summary>
        public static object GetPropertyValue(this object instance, string propertyName)
        {
            return instance.GetType().GetInstanceProperty(propertyName).GetValue(instance);
        }


        /// <summary>
        /// Set static property value.
        /// </summary>
        public static void SetPropertyValue(this Type type, string propertyName, object value) { type.GetStaticProperty(propertyName).SetValue(null, value); }


        /// <summary>
        /// Get static property value.
        /// </summary>
        public static object GetPropertyValue(this Type type, string propertyName) { return type.GetStaticProperty(propertyName).GetValue(null); }


        /// <summary>
        /// Invoke an Instance method.
        /// </summary>
        public static object InvokeMethod(this object instance, string methodName, params object[] parameters)
        {
            return instance.GetType().GetInstanceMethod(methodName).Invoke(instance, parameters);
        }


        /// <summary>
        /// Invoke an static method.
        /// </summary>
        public static object InvokeMethod(this Type type, string methodName, params object[] parameters)
        {
            return type.GetStaticMethod(methodName).Invoke(null, parameters);
        }
        
        public static FieldInfo GetFieldViaPath(this Type type, string path, BindingFlags flags)
        {
            var parent = type;
            var fi = parent.GetField(path, flags);
            var paths = path.Split('.');
     
            for (int i = 0; i < paths.Length; i++)
            {
                fi = parent.GetField(paths[i], flags);
                if (fi != null)
                {
                    // there are only two container field type that can be serialized:
                    // Array and List<T>
                    if (fi.FieldType.IsArray)
                    {
                        parent = fi.FieldType.GetElementType();
                        i += 2;
                        continue;
                    }
     
                    if (fi.FieldType.IsGenericType)
                    {
                        parent = fi.FieldType.GetGenericArguments()[0];
                        i += 2;
                        continue;
                    }
                    parent = fi.FieldType;
                }
                else
                {
                    break;
                }
     
            }
            if (fi == null)
            {
                if (type.BaseType != null)
                {
                    return GetFieldViaPath(type.BaseType, path, flags);
                }

                return null;
            }
            return fi;
        }
    }
}