using System;
using System.Reflection;
using Pancake.ExLib.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for Apex callbacks.
    /// </summary>
    public static class ApexCallbackUtility
    {
        public static T GetCallbackResult<T>(object target, string name)
        {
            var returnType = typeof(T);
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
            {
                if (memberInfo.Name == name)
                {
                    if (memberInfo is FieldInfo fieldInfo && fieldInfo.FieldType == returnType)
                    {
                        return (T) fieldInfo.GetValue(target);
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.PropertyType == returnType)
                    {
                        return (T) propertyInfo.GetValue(target);
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.ReturnType == returnType && methodInfo.GetParameters().Length == 0)
                    {
                        return (T) methodInfo.Invoke(target, null);
                    }
                }
            }

            Debug.LogError($"<b>Apex Exception:</b> Method with name {name} is not founded in {type.Name}.");
            return default;
        }

        /// <summary>
        /// Check method info properties.
        /// </summary>
        /// <param name="methodInfo">This method info.</param>
        /// <param name="name">Callback name, set null or empty to ignore.</param>
        /// <param name="returnType">Desired return type.</param>
        /// <param name="parameterTypes">Desired parameter types.</param>
        /// <returns>True if all properties meet the requirements. Otherwise false.</returns>
        public static bool IsValidCallback(this MethodInfo methodInfo, string name, Type returnType, params Type[] parameterTypes)
        {
            if (methodInfo.Name == name && methodInfo.ReturnType == returnType)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if ((parameterTypes == null || parameterTypes.Length == 0) && parameters.Length == 0)
                {
                    return true;
                }
                else if (parameterTypes != null && (parameterTypes.Length == parameters.Length))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != parameterTypes[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}