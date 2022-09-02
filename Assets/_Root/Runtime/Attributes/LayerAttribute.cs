using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// Show the int value as layer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class LayerAttribute : PropertyAttribute
    {
#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(LayerAttribute))]
        class LayerDrawer : BasePropertyDrawer<LayerAttribute>
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
                        break;
                    case SerializedPropertyType.String:
                        property.stringValue = LayerMask.LayerToName(EditorGUI.LayerField(position, label, LayerMask.NameToLayer(property.stringValue)));
                        break;
                    default:
                        EditorGUI.LabelField(position, label.text,"Use LayerAttribute must be an int or string" );
                        break;
                }
            }
        } // class LayerDrawer

#endif // UNITY_EDITOR
    } // LayerAttribute
} // namespace Pancake