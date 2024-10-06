using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Sisus.Init.Internal.TypeUtility;

namespace Sisus.Init.EditorOnly.Internal
{
    /// <summary>
    /// Extension methods for <see cref="SerializedProperty"/>.
    /// </summary>
    internal static class SerializedPropertyExtensions
    {
        private static readonly Dictionary<Type, bool> hasFoldoutCache = new();

        internal static bool HasFoldoutInInspector([DisallowNull] this SerializedProperty property, [DisallowNull] Type valueType)
        {
            if(!property.hasChildren)
            {
                return false;
            }

            if(property.isExpanded)
            {
                return true;
            }

            if(!hasFoldoutCache.TryGetValue(valueType, out bool hasFoldout))
            {
                hasFoldout = property.propertyType switch
                {
                    SerializedPropertyType.Generic or SerializedPropertyType.ManagedReference => IsSerializableByUnity(valueType) && HasAnySerializedFields(valueType),
                    _ => false,
                };

                hasFoldoutCache.Add(valueType, hasFoldout);
            }

            return hasFoldout;

            static bool HasAnySerializedFields([DisallowNull] Type valueType)
            {
                for(var type = valueType; !IsNullOrBaseType(type); type = type.BaseType)
                {
                    foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if((field.IsPublic && IsSerializableByUnity(field.FieldType)
                            || field.GetCustomAttribute<SerializeField>() is not null)
                           && field.GetCustomAttribute<HideInInspector>() is null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}