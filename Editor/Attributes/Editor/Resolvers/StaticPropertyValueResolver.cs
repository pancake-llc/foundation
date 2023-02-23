using System;
using System.Reflection;
using UnityEngine;

namespace Pancake.AttributeDrawer
{
    public class StaticPropertyValueResolver<T> : ValueResolver<T>
    {
        private readonly PropertyInfo _propertyInfo;

        public static bool TryResolve(PropertyDefinition propertyDefinition, string expression, out ValueResolver<T> resolver)
        {
            if (expression.IndexOf('.') == -1)
            {
                resolver = null;
                return false;
            }

            var separatorIndex = expression.LastIndexOf('.');
            var className = expression.Substring(0, separatorIndex);
            var methodName = expression.Substring(separatorIndex + 1);

            if (!ReflectionUtilities.TryFindTypeByFullName(className, out var type))
            {
                resolver = null;
                return false;
            }

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var propertyInfo in type.GetProperties(flags))
            {
                if (propertyInfo.Name == methodName && typeof(T).IsAssignableFrom(propertyInfo.PropertyType) && propertyInfo.CanRead)
                {
                    resolver = new StaticPropertyValueResolver<T>(propertyInfo);
                    return true;
                }
            }

            resolver = null;
            return false;
        }

        public StaticPropertyValueResolver(PropertyInfo propertyInfo) { _propertyInfo = propertyInfo; }

        public override bool TryGetErrorString(out string error)
        {
            error = "";
            return false;
        }

        public override T GetValue(Property property, T defaultValue = default)
        {
            try
            {
                return (T) _propertyInfo.GetValue(null);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException targetInvocationException)
                {
                    e = targetInvocationException.InnerException;
                }

                Debug.LogException(e);
                return defaultValue;
            }
        }
    }
}