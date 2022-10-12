using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ToggleLeftAttribute))]
    sealed class ToggleLeftView : FieldView
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            element.serializedProperty.boolValue = EditorGUI.ToggleLeft(position, label, element.serializedProperty.boolValue);
        }
    }
}