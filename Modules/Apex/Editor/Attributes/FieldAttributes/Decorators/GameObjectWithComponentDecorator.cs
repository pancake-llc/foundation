using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(GameObjectWithComponentAttribute))]
    public sealed class GameObjectWithComponentDecorator : FieldDecorator, ITypeValidationCallback
    {
        private float width;
        private GUIContent content;
        private SerializedProperty property;
        private GameObjectWithComponentAttribute attribute;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="field">Serialized element reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField field, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            property = field.GetSerializedProperty();

            attribute = (GameObjectWithComponentAttribute) decoratorAttribute;

            string text = attribute.Format;
            text = text.Replace("{name}", property.displayName);
            text = text.Replace("{type}", attribute.type.Name);
            content = new GUIContent(text);
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI(Rect position)
        {
            width = position.width;
            EditorGUI.HelpBox(position, content.text, HelpBoxDecorator.CovertStyleToType(attribute.Style));
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        /// <param name="element">Reference of serialized element decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override float GetHeight() { return EditorStyles.helpBox.CalcHeight(content, width); }

        /// <summary>
        /// Field decorator visibility state.
        /// </summary>
        public override bool IsVisible() { return property.objectReferenceValue is GameObject gameObject && gameObject.GetComponent(attribute.type) == null; }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ObjectReference; }
    }
}