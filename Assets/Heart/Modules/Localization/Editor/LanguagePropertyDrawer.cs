using Pancake.Localization;
using UnityEditor;
using UnityEngine;

namespace Pancake.LocalizationEditor
{
    [CustomPropertyDrawer(typeof(Language))]
    public class LanguagePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { Helper.LanguageField(position, property, label); }
    }
}