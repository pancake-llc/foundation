using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vexe.Runtime.Extensions;

namespace Pancake.ApexEditor
{
    [DecoratorTarget(typeof(RectangleSpaceAttribute))]
    public sealed class RectangleSpaceDecorator : FieldDecorator
    {
        private RectangleSpaceAttribute attribute;

        // Stored callbacks.
        private object target;
        private MethodCaller<object, object> onGUI;
        private MethodCaller<object, object> getHeight;

        /// <summary>
        /// Called when element decorator becomes initialized.
        /// </summary>
        /// <param name="serializedField">serialized field reference with current decorator attribute.</param>
        /// <param name="decoratorAttribute">Reference of serialized field decorator attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, DecoratorAttribute decoratorAttribute, GUIContent label)
        {
            attribute = decoratorAttribute as RectangleSpaceAttribute;

            target = serializedField.GetDeclaringObject();
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                if (onGUI != null && (string.IsNullOrEmpty(attribute.GetHeightCallback) || getHeight != null))
                {
                    break;
                }

                if (onGUI == null && methodInfo.IsValidCallback(attribute.name, typeof(void), typeof(Rect)))
                {
                    onGUI = methodInfo.DelegateForCall();
                }
                else if (getHeight == null && methodInfo.IsValidCallback(attribute.GetHeightCallback, typeof(float)))
                {
                    getHeight = methodInfo.DelegateForCall<object, object>();
                }
            }
        }

        /// <summary>
        /// Called for rendering and handling decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing decorator.</param>
        public override void OnGUI(Rect position)
        {
            if (onGUI != null)
                onGUI.Invoke(target, new object[1] {position});
            else
                EditorGUI.HelpBox(position, $"<b>Apex Message:</b> Method with {attribute.name} for Rectangle Space is not found.", MessageType.Error);
        }

        /// <summary>
        /// Get the height of the decorator, which required to display it.
        /// Calculate only the size of the current decorator, not the entire property.
        /// The decorator height will be added to the total size of the property with other decorator.
        /// </summary>
        public override float GetHeight()
        {
            if (getHeight != null)
                return (float) getHeight.Invoke(target, null);
            else
                return attribute.Height;
        }
    }
}