#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace Pancake.Editor
{
    public class BasePropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// BasePropertyDrawer
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);
        }
    } // BasePropertyDrawer

    /// <summary>
    /// BasePropertyDrawer<typeparamref name="T"/>>
    /// </summary>
    public class BasePropertyDrawer<T> : BasePropertyDrawer where T : PropertyAttribute
    {
        // ReSharper disable once InconsistentNaming
        protected new T attribute => (T) base.attribute;

        protected static IEnumerable<FieldInfo> GetAllFields(object target)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    ;//.Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        protected static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<PropertyInfo> propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        protected static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            List<Type> types = GetSelfAndBaseTypes(target);

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<MethodInfo> methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }

        // protected static FieldInfo GetField(object target, string fieldName)
        // {
        //     return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
        // }

        protected static PropertyInfo GetProperty(object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName, StringComparison.Ordinal)).FirstOrDefault();
        }

        protected static MethodInfo GetMethod(object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
        }

        protected static Type GetListElementType(Type listType)
        {
            if (listType.IsGenericType)
            {
                return listType.GetGenericArguments()[0];
            }
            else
            {
                return listType.GetElementType();
            }
        }

        /// <summary>
        ///		Get type and all base types of target, sorted as following:
        ///		<para />[target's type, base type, base's base type, ...]
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            List<Type> types = new List<Type>() {target.GetType()};

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }
    }
}

#endif // UNITY_EDITOR