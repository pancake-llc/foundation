using Pancake.Localization;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Localization
{
    [CustomPropertyDrawer(typeof(Language))]
    public class LanguagePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { LocaleEditorUtil.LanguageField(position, property, label); }
    }
}