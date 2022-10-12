using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(GameObjectWithComponentAttribute))]
    sealed class GameObjectWithComponentDecorator : FieldDecorator, ITypeValidationCallback
    {
        private GameObjectWithComponentAttribute attribute;
        private SerializedField element;
        private GUIContent content;
        private Type type;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            this.element = element;
            attribute = decoratorAttribute as GameObjectWithComponentAttribute;
            type = attribute.type;
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI(Rect position) { EditorGUI.HelpBox(position, content.text, HelpBoxDecorator.CovertStyleToType(attribute.Style)); }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        /// <param name="element">Reference of serialized element decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override float GetHeight()
        {
            if (CheckComponent())
            {
                string format = attribute.Format.Replace("{name}", element.GetObject().name).Replace("{type}", type.Name);
                content = new GUIContent(format);
                if (attribute.Height == 0)
                {
                    GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                    Vector2 size = style.CalcSize(content);
                    attribute.Height = style.CalcHeight(content, size.x);
                }

                return attribute.Height;
            }

            return 0;
        }

        /// <summary>
        /// Check that game object field contains required component.
        /// </summary>
        /// <returns>True if component contains, otherwise false.</returns>
        private bool CheckComponent()
        {
            GameObject gameObject = element.GetObject() as GameObject;
            if (gameObject != null)
            {
                return gameObject.GetComponent(type) != null;
            }

            return true;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue is GameObject;
        }
    }
}