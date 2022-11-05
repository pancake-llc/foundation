using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DecoratorTarget(typeof(RectangleSpaceAttribute))]
    sealed class RectangleSpaceDecorator : FieldDecorator
    {
        private RectangleSpaceAttribute attribute;
        private MethodInfo method;
        private object target;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as RectangleSpaceAttribute;

            target = serializedField.GetSerializedObject().targetObject;
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            method = methods.Where(m =>
                    m.Name == attribute.method && m.ReturnType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Rect))
                .FirstOrDefault();
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (method != null)
            {
                method.Invoke(target, new object[1] {position});
            }
            else
            {
                EditorGUI.HelpBox(position, "OnGUI method not found!", MessageType.Error);
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