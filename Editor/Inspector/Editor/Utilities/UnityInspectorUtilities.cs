using System.Reflection;
using UnityEngine;

namespace Pancake.Editor
{
    public class UnityInspectorUtilities
    {
        private static readonly FieldInfo GUIStyleNameBackingField = typeof(GUIStyle).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool MustDrawWithUnity(Property property)
        {
            if (property.FieldType == typeof(GUIStyle) || property.FieldType == typeof(RectOffset))
            {
                return true;
            }

            return !property.IsArray && property.TryGetAttribute(out DrawWithUnityAttribute _);
        }

        public static bool TryGetSpecialArrayElementName(Property property, out string name)
        {
            if (property.FieldType == typeof(GUIStyle) && property.Value is GUIStyle guiStyle)
            {
                GUIStyleNameBackingField?.SetValue(guiStyle, null);
                name = guiStyle.name;
                return true;
            }

            if (property.PropertyType == EPropertyType.Generic && property.ChildrenProperties.Count > 0 && property.ChildrenProperties[0] is var firstChild &&
                firstChild.ValueType == typeof(string) && firstChild.Value is string firstChildValueStr)
            {
                name = firstChildValueStr;
                return true;
            }

            name = default;
            return false;
        }
    }
}