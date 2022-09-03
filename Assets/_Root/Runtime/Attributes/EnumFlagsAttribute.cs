using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// Use on an enum field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class EnumFlagsAttribute : PropertyAttribute
    {
        private bool _includeObsolete;

        public EnumFlagsAttribute(bool includeObsolete = false) { _includeObsolete = includeObsolete; }


#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
        private class EnumFlagsDrawer : BasePropertyDrawer<EnumFlagsAttribute>
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var value = (Enum) Enum.ToObject(fieldInfo.FieldType, property.intValue);

                using (var scope = ChangeCheckScope.New())
                {
                    value = EditorGUI.EnumFlagsField(position, label, value, attribute._includeObsolete);
                    if (scope.changed)
                    {
                        property.intValue = value.GetHashCode();
                    }
                }
            }
        } // class EnumFlagsDrawer

#endif // UNITY_EDITOR
    } // class EnumFlagsAttribute
} // namespace Pancake