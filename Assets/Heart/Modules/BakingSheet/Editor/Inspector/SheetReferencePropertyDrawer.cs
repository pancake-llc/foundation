using Pancake.BakingSheet;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.BakingSheet
{
    [CustomPropertyDrawer(typeof(Sheet<,>.Reference), true)]
    public class SheetReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetProp = property.FindPropertyRelative("asset");

            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            EditorGUI.PropertyField(position, assetProp, new GUIContent(property.displayName));
        }
    }
}