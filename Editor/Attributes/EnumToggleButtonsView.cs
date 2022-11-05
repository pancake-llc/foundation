using UnityEditor;
using UnityEngine;


namespace Pancake.Editor
{
    [ViewTarget(typeof(EnumToggleButtonsAttribute))]
    sealed class EnumToggleButtonsView : FieldView, ITypeValidationCallback
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            int index = serializedField.GetSerializedProperty().enumValueIndex;
            index = GUI.Toolbar(position, index, serializedField.GetSerializedProperty().enumDisplayNames);
            serializedField.GetSerializedProperty().enumValueIndex = index;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Enum; }
    }
}