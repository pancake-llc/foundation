using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Attribute
{
    public abstract class BuiltinDrawerBase<T> : ValueDrawer<T>
    {
        public sealed override InspectorElement CreateElement(InspectorValue<T> propertyValue, InspectorElement next)
        {
            if (propertyValue.Property.TryGetSerializedProperty(out _))
            {
                return next;
            }

            return base.CreateElement(propertyValue, next);
        }

        public virtual int CompactModeLines => 1;
        public virtual int WideModeLines => 1;

        public sealed override float GetHeight(float width, InspectorValue<T> propertyValue, InspectorElement next)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var lines = EditorGUIUtility.wideMode ? WideModeLines : CompactModeLines;
            return lineHeight * lines + spacing * (lines - 1);
        }

        public sealed override void OnGUI(Rect position, InspectorValue<T> propertyValue, InspectorElement next)
        {
            var value = propertyValue.SmartValue;

            EditorGUI.BeginChangeCheck();

            value = OnValueGUI(position, propertyValue.Property.DisplayNameContent, value);

            if (EditorGUI.EndChangeCheck())
            {
                propertyValue.SmartValue = value;
            }
        }

        protected abstract T OnValueGUI(Rect position, GUIContent label, T value);
    }
}