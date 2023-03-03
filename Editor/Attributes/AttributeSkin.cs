using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Attribute
{
    internal struct AttributeSkin
    {
        public static void Foldout(Rect rect, Property property)
        {
            var content = property.DisplayNameContent;
            property.IsExpanded = EditorGUI.Foldout(rect, property.IsExpanded, content, true);
        }

        public static GUIStyle TabOnlyOne { get; } = "Tab onlyOne";
        public static GUIStyle TabFirst { get; } = "Tab first";
        public static GUIStyle TabMiddle { get; } = "Tab middle";
        public static GUIStyle TabLast { get; } = "Tab last";
    }
}