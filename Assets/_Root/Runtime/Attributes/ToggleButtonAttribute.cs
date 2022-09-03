using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// Show a bool as toggle button
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ToggleButtonAttribute : PropertyAttribute
    {
        public ToggleButtonAttribute(string text, bool indent = true)
        {
#if UNITY_EDITOR
            _label = text;
            _indent = indent;
#endif
        }


        public ToggleButtonAttribute(string label, string trueText, string falseText)
        {
#if UNITY_EDITOR
            _label = label;
            _trueText = trueText;
            _falseText = falseText;
#endif
        }

#if UNITY_EDITOR

        private string _label;
        private string _trueText;
        private string _falseText;
        private bool _indent;


        [CustomPropertyDrawer(typeof(ToggleButtonAttribute))]
        private class ToggleButtonDrawer : BasePropertyDrawer<ToggleButtonAttribute>
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label.text = attribute._label;
                if (attribute._trueText != null && attribute._falseText != null)
                {
                    position = EditorGUI.PrefixLabel(position, label);
                    if (GUI.Button(position, property.boolValue ? attribute._trueText : attribute._falseText, EditorStyles.miniButton))
                    {
                        property.boolValue = !property.boolValue;
                    }
                }
                else
                {
                    if (attribute._indent) position.xMin += EditorGUIUtility.labelWidth;
                    property.boolValue = GUI.Toggle(position, property.boolValue, label, EditorStyles.miniButton);
                }
            }
        } // class ToggleButtonDrawer

#endif // UNITY_EDITOR
    } // class ToggleButtonAttribute
} // namespace Pancake