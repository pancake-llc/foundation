using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(FieldButtonAttribute))]
    sealed class FieldButtonDecorator : FieldDecorator
    {
        private object target;
        private FieldButtonAttribute attribute;
        private MethodInfo action;
        private GUIContent content;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="attribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        public override void Initialize(SerializedField element, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            target = element.serializedObject.targetObject;
            attribute = decoratorAttribute as FieldButtonAttribute;
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            action = methods.Where(m => m.Name == attribute.method && m.GetParameters().Length == 0).FirstOrDefault();

            content = new GUIContent();
            if (!string.IsNullOrEmpty(attribute.Label))
            {
                content.text = attribute.Label;
                if (attribute.Label[0] == '@')
                {
                    content = EditorGUIUtility.IconContent(attribute.Label.Remove(0, 1));
                }
            }
            else if (action != null)
            {
                content.text = action.Name;
            }
            else
            {
                content = GUIContent.none;
            }
        }

        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        public override void OnGUI(Rect position)
        {
            if (GUI.Button(position, content, attribute.Style))
            {
                action?.Invoke(target, null);
            }
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight() { return attribute.Height; }
    }
}