using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Pancake.Init.EditorOnly
{
    internal class DropdownButton<T>
    {
        internal readonly GUIContent prefixLabel;
        internal readonly GUIContent buttonLabel;        

        private readonly Action<Rect> openDropdown;
        private readonly bool highlightMissingValue;

        public DropdownButton(string prefixLabel, string buttonLabel, [NotNull] Action<Rect> openDropdown, bool highlightMissingValue)
            : this(new GUIContent(prefixLabel), new GUIContent(buttonLabel), openDropdown, highlightMissingValue) { }

        public DropdownButton(GUIContent prefixLabel, GUIContent buttonLabel, [NotNull] Action<Rect> openDropdown, bool highlightMissingValue)
        {
            this.prefixLabel = prefixLabel;
            this.buttonLabel = buttonLabel;
            this.openDropdown = openDropdown;
            this.highlightMissingValue = highlightMissingValue;
        }

        public void Draw(Rect position)
        {
            var buttonPosition = prefixLabel.text.Length > 0 ? EditorGUI.PrefixLabel(position, prefixLabel) : position;

            Color guiColorWas = GUI.color;
            if(highlightMissingValue && (string.Equals(buttonLabel.text, "None") || string.Equals(buttonLabel.text, "Null")))
            {
                GUI.color = Color.red;
            }

            if(EditorGUI.DropdownButton(buttonPosition, buttonLabel, FocusType.Keyboard))
            {
                GUI.color = Color.white;
                openDropdown(buttonPosition);
            }

            GUI.color = guiColorWas;
        }
    }
}