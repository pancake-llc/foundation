using System;
using System.Reflection;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public static class ReflectionExtension
    {
        public static object[] Void => Array.Empty<object>();
        public const BindingFlags PRIVATE_FLAG = BindingFlags.NonPublic | BindingFlags.Instance;

        private static bool HasBindingFlags(BindingFlags flags) => flags != BindingFlags.Default;

        public static T GetProperty<T>(string propertyName, Type targetType, object obj, BindingFlags flags = BindingFlags.Default)
        {
            return (T) GetProperty(propertyName, targetType, obj, flags);
        }

        public static object GetProperty(string propertyName, Type targetType, object obj, BindingFlags flags = BindingFlags.Default)
        {
            return GetPropertyInfo(propertyName, targetType, flags).GetValue(obj);
        }

        public static void SetProperty(string propertyName, Type targetType, object target, object value, BindingFlags flags = BindingFlags.Default)
        {
            var property = GetPropertyInfo(propertyName, targetType, flags);
            property.SetValue(target, value);
        }

        private static PropertyInfo GetPropertyInfo(string propertyName, Type targetType, BindingFlags flags)
        {
            return HasBindingFlags(flags) ? targetType?.GetProperty(propertyName, flags) : targetType?.GetProperty(propertyName);
        }

        public static T GetField<T>(string fieldName, Type targetType, object obj, BindingFlags flags = BindingFlags.Default)
        {
            return (T) GetField(fieldName, targetType, obj, flags);
        }

        public static object GetField(string fieldName, Type targetType, object obj, BindingFlags flags = BindingFlags.Default)
        {
            var field = GetFieldInfo(fieldName, targetType, flags);
            return field.GetValue(obj);
        }

        public static void SetField(string fieldName, Type targetType, object target, object value, BindingFlags flags = BindingFlags.Default)
        {
            var field = GetFieldInfo(fieldName, targetType, flags);
            field.SetValue(target, value);
        }

        private static FieldInfo GetFieldInfo(string fieldName, Type targetType, BindingFlags flags)
        {
            return HasBindingFlags(flags) ? targetType?.GetField(fieldName, flags) : targetType?.GetField(fieldName);
        }

        public static object ExecuteMethod(string methodName, object[] parameter, Type executorType, object executor, BindingFlags flags = BindingFlags.Default)
        {
            var method = HasBindingFlags(flags) ? executorType?.GetMethod(methodName, flags) : executorType?.GetMethod(methodName);
            return method.Invoke(executor, parameter);
        }

        public static object CreateNewObjectWithReflection(Type type, object[] constructorParameters, BindingFlags flags = BindingFlags.Default)
        {
            var constructors = HasBindingFlags(flags) ? type?.GetConstructors(flags) : type?.GetConstructors();

            if (constructors == null)
            {
                Debug.LogError($"Can't get {type.FullName}'s constructor");
                return null;
            }

            if (constructors.Length == 0) return Activator.CreateInstance(type);

            object result = null;
            for (var i = 0; i < constructors.Length; i++)
            {
                try
                {
                    result = constructors[i].Invoke(constructorParameters);
                }
                catch
                {
                    //
                }
            }

            return result;
        }
    }
}