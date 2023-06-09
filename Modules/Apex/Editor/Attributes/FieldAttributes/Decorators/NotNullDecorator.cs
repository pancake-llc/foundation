using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(NotNullAttribute))]
    public sealed class NotNullDecorator : FieldDecorator, ITypeValidationCallback
    {
        private NotNullAttribute attribute;
        private SerializedProperty serializedProperty;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as NotNullAttribute;
            attribute.Format = attribute.Format.Replace("{name}", label.text);
            serializedProperty = serializedField.GetSerializedProperty();
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position) { EditorGUI.HelpBox(position, attribute.Format, HelpBoxDecorator.CovertStyleToType(attribute.Style)); }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight() { return attribute.Height; }

        /// <summary>
        /// Field decorator visibility state.
        /// </summary>
        public override bool IsVisible() { return serializedProperty.objectReferenceValue == null; }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ObjectReference; }
    }
}