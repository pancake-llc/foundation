using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(NotEmptyAttribute))]
    sealed class NotEmptyDecorator : FieldDecorator, ITypeValidationCallback
    {
        private NotEmptyAttribute attribute;
        private SerializedProperty serializedProperty;

        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as NotEmptyAttribute;
            attribute.Format = attribute.Format.Replace("{name}", label.text);
            serializedProperty = element.serializedProperty;
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (string.IsNullOrEmpty(serializedProperty.stringValue))
            {
                EditorGUI.HelpBox(position, attribute.Format, HelpBoxDecorator.CovertStyleToType(attribute.Style));
            }
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight()
        {
            if (string.IsNullOrEmpty(serializedProperty.stringValue))
            {
                return attribute.Height > 0 ? attribute.Height : EditorGUIUtility.singleLineHeight;
            }

            return 0;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.String; }
    }
}