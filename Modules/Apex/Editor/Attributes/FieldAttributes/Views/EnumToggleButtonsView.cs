using UnityEditor;
using UnityEngine;
using Pancake.Apex;
using System.Reflection;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(EnumToggleButtonsAttribute))]
    public sealed class EnumToggleButtonsView : FieldView, ITypeValidationCallback
    {
        private float width;
        private bool isFlags;
        private GUIContent helpBoxContent;

        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            isFlags = serializedField.GetMemberType().GetCustomAttribute<System.FlagsAttribute>() != null;
            if (isFlags)
            {
                helpBoxContent = new GUIContent("[EnumToggleButtons] attribute does not support enum types with [Flags] attribute.");
            }
            else
            {
                helpBoxContent = GUIContent.none;
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (!isFlags)
            {
                position = EditorGUI.PrefixLabel(position, label);
                int index = serializedField.GetSerializedProperty().enumValueIndex;
                index = GUI.Toolbar(position, index, serializedField.GetSerializedProperty().enumDisplayNames);
                serializedField.GetSerializedProperty().enumValueIndex = index;
            }
            else
            {
                width = position.width;
                EditorGUI.HelpBox(position, helpBoxContent.text, MessageType.Error);
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            if (isFlags)
            {
                return EditorStyles.helpBox.CalcHeight(helpBoxContent, width);
            }

            return base.GetHeight(serializedField, label);
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Enum; }
    }
}