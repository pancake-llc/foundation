using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sisus.Shared.EditorOnly
{
    public static class PropertyDrawerUtility
    {
        public static PropertyAttribute GetAttribute(PropertyDrawer propertyDrawer) => propertyDrawer.m_Attribute;
        public static void SetAttribute(PropertyDrawer propertyDrawer, PropertyAttribute propertyAttribute) => propertyDrawer.m_Attribute = propertyAttribute;

        public static FieldInfo GetFieldInfo(PropertyDrawer propertyDrawer) => propertyDrawer.m_FieldInfo;
        public static void SetFieldInfo(PropertyDrawer propertyDrawer, FieldInfo fieldInfo) => propertyDrawer.m_FieldInfo = fieldInfo;

        public static Type GetTargetType(CustomPropertyDrawer customPropertyDrawer) => customPropertyDrawer.m_Type;
        public static void SetTargetType(CustomPropertyDrawer customPropertyDrawer, Type targetType) => customPropertyDrawer.m_Type = targetType;

        public static bool GetUseForChildren(CustomPropertyDrawer customPropertyDrawer) => customPropertyDrawer.m_UseForChildren;
        public static void SetUseForChildren(CustomPropertyDrawer customPropertyDrawer, bool useForChildren) => customPropertyDrawer.m_UseForChildren = useForChildren;
    }
}