using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
using System.Reflection;
#endif

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SortingLayerAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
        class SortingLayerDrawer : BasePropertyDrawer<SortingLayerAttribute>
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return (property.propertyType == SerializedPropertyType.String || property.propertyType == SerializedPropertyType.Integer)
                    ? EditorGUI.GetPropertyHeight(property)
                    : EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.singleLineHeight;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        DrawPropertyForString(position, property, label, GetLayers());
                        break;
                    case SerializedPropertyType.Integer:
                        DrawPropertyForInt(position, property, label, GetLayers());
                        break;
                    default:
                        EditorGUI.HelpBox(position, string.Format("{0} must be an int or a string", property.name), MessageType.Warning);
                        break;
                }

                EditorGUI.EndProperty();
            }

            private string[] GetLayers()
            {
                Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
                PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
                return (string[]) sortingLayersProperty.GetValue(null, new object[0]);
            }

            private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label, string[] layers)
            {
                int index = IndexOf(layers, property.stringValue);
                int newIndex = EditorGUI.Popup(rect, label.text, index, layers);
                string newLayer = layers[newIndex];

                if (!property.stringValue.Equals(newLayer, StringComparison.Ordinal))
                {
                    property.stringValue = layers[newIndex];
                }
            }

            private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label, string[] layers)
            {
                int index = 0;
                string layerName = SortingLayer.IDToName(property.intValue);
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layerName.Equals(layers[i], StringComparison.Ordinal))
                    {
                        index = i;
                        break;
                    }
                }

                int newIndex = EditorGUI.Popup(rect, label.text, index, layers);
                string newLayerName = layers[newIndex];
                int newLayerNumber = SortingLayer.NameToID(newLayerName);

                if (property.intValue != newLayerNumber)
                {
                    property.intValue = newLayerNumber;
                }
            }

            private static int IndexOf(string[] layers, string layer)
            {
                var index = Array.IndexOf(layers, layer);
                return Mathf.Clamp(index, 0, layers.Length - 1);
            }
        }
#endif
    }
}